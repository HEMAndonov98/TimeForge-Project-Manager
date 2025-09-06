using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeForge.Common.Enums;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;

namespace TimeForge.Services;

public class UserConnectionService : IConnectionService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly ILogger<UserConnectionService> logger;
    private readonly UserManager<User> userManager;
    
    public UserConnectionService(ITimeForgeRepository timeForgeRepository, ILogger<UserConnectionService> logger, UserManager<User> userManager)
    {
        this.timeForgeRepository = timeForgeRepository;
        this.logger = logger;
        this.userManager = userManager;
    }   
    
    
    public async Task SendConnectionAsync(string fromUserId, string toUserEmail)
    {
        this.logger.LogInformation("Sending new user connection");
        
        this.logger.LogInformation($"Retrieving user ID for To user with email: {toUserEmail}");
        string? toUserId = (await this.userManager.FindByEmailAsync(toUserEmail))?.Id;

        if (String.IsNullOrEmpty(toUserId))
        {
            this.logger.LogError("to user not found, invalid email or non existent user");
            throw new ArgumentException("to user not found, invalid email or non existent user");
        }

        UserConnection userConnection = this.CreateConnection(fromUserId, toUserId);
        await this.timeForgeRepository.AddAsync(userConnection);
        await this.timeForgeRepository.SaveChangesAsync();
        this.logger.LogInformation("New user connection added to database");
    }

    public UserConnection CreateConnection(string fromUserId, string toUserId)
    {
        this.logger.LogInformation("Creating a new connection");
        
        bool connectionExists = this.CheckConnectionExists(fromUserId, toUserId);
        
        if (connectionExists)
        {
            this.logger.LogError("Connection already exists or parameters are invalid");
            throw new InvalidOperationException("Connection already exists or parameters are invalid");
        }
        
        this.logger.LogInformation("this connection does not exist and parameters are valid");
        UserConnection newUserConnection = new UserConnection()
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Status = ConnectionStatus.Pending
        };
        this.logger.LogInformation("New connection created");
        return newUserConnection;
    }

    public async Task UpdateConnectionAsync(UserConnection userConnection, ConnectionStatus status)
    {
        this.logger.LogInformation($"Updating user connection with composite key: {{ {userConnection.ToUserId} , {userConnection.FromUserId} }}");
        UserConnection? foundUserConnection = await this.timeForgeRepository.All<UserConnection>()
            .Where(uc =>
                uc.ToUserId == userConnection.ToUserId &&
                uc.FromUserId == userConnection.FromUserId)
            .FirstOrDefaultAsync();

        if (foundUserConnection == null)
        {
            this.logger.LogError("User connection not found");
            throw new ArgumentException("User connection not found");
        }

        foundUserConnection.Status = status;
        this.logger.LogInformation("User connection status property updated to:  {ConnectionStatus}", status);
        
        this.timeForgeRepository.Update(foundUserConnection);
        await this.timeForgeRepository.SaveChangesAsync();
        this.logger.LogInformation("User connection updated");
    }

    /// <summary>
    /// Validates and checks if users with this id exist in the database
    /// </summary>
    /// <param name="fromUserId">Id representing the user who is sending the request</param>
    /// <param name="toUserId">Id representing the user who will receive the request</param>
    /// <returns>true if the statement is not valid and false if it is valid</returns>
    public bool CheckConnectionExists(string fromUserId, string toUserId)
    {
        this.logger.LogInformation("Checking if user connection exists");
        if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(toUserId))
        {
            this.logger.LogWarning(
                "UserConnectionService: CheckConnectionExists: fromUserId or toUserId is null or empty");
            return true;
        }

        if (!this.userManager.Users.Any(u => u.Id == fromUserId))
        {
            this.logger.LogError($"UserConnectionService: CheckConnectionExists: no user exists with id: {fromUserId}");
            return true;
        }

        if (!this.userManager.Users.Any(u => u.Id == toUserId))
        {
            this.logger.LogError($"UserConnectionService: CheckConnectionExists: no user exists with id {toUserId}");
            return true;
        }

        this.logger.LogInformation("UserConnectionService: CheckConnectionExists");
        return false;
    }
}