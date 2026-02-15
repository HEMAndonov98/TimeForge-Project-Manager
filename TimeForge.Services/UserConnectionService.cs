using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeForge.Common.Enums;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.UserConnection;

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
    
    /// <summary>
    /// Sends a connection request from one user to another based on their identifiers.
    /// </summary>
    /// <param name="fromUserId">The ID of the user initiating the connection request.</param>
    /// <param name="toUserEmail">The email address of the user receiving the request.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the recipient user is not found or the email provided is invalid.
    /// </exception>
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

        Friendship friendship = this.CreateConnection(fromUserId, toUserId);
        await this.timeForgeRepository.AddAsync(friendship);
        await this.timeForgeRepository.SaveChangesAsync();
        this.logger.LogInformation("New user connection added to database");
    }
    
    /// <summary>
    /// Creates and returns a new user connection object.
    /// </summary>
    /// <param name="fromUserId">The ID of the user initiating the connection.</param>
    /// <param name="toUserId">The ID of the user receiving the connection request.</param>
    /// <returns>A new instance of <see cref="Friendship"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a connection already exists or when input parameters are invalid.
    /// </exception>
    public Friendship CreateConnection(string fromUserId, string toUserId)
    {
        this.logger.LogInformation("Creating a new connection");

        bool connectionExists = this.CheckConnectionExists(fromUserId, toUserId);

        if (connectionExists)
        {
            this.logger.LogError("Connection already exists or parameters are invalid");
            throw new InvalidOperationException("Connection already exists or parameters are invalid");
        }

        this.logger.LogInformation("this connection does not exist and parameters are valid");
        Friendship newFriendship = new Friendship()
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Status = FriendshipStatus.Pending
        };
        this.logger.LogInformation("New connection created");
        return newFriendship;
    }
    
    /// <summary>
    /// Updates the status of a user connection
    /// </summary>
    /// <param name="friendship">The user connection that will be updated</param>
    /// <param name="status">The new status of the user connection</param>
    /// <exception cref="ArgumentException">Throws an ArgumentException if Friendship does not exist in the database</exception>
    public async Task UpdateConnectionAsync(Friendship friendship, FriendshipStatus status)
    {
        this.logger.LogInformation($"Updating user connection with composite key: {{ {friendship.ToUserId} , {friendship.FromUserId} }}");
        Friendship? foundUserConnection = await this.timeForgeRepository.All<Friendship>()
            .Where(uc =>
                uc.ToUserId == friendship.ToUserId &&
                uc.FromUserId == friendship.FromUserId)
            .FirstOrDefaultAsync();

        if (foundUserConnection == null)
        {
            this.logger.LogError("User connection not found");
            throw new ArgumentException("User connection not found");
        }

        foundUserConnection.Status = status;
        this.logger.LogInformation("User connection status property updated to:  {FriendshipStatus}", status);
        
        this.timeForgeRepository.Update(foundUserConnection);
        await this.timeForgeRepository.SaveChangesAsync();
        this.logger.LogInformation("User connection updated");
    }

    public async Task<UserConnectionViewModel> GetConnectionsByUserIdAsync(string userId)
    {
        var user = await this.userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User does not exist");
        }
        
        var acceptedUserConnections = await this.GetAcceptedUserConnectionsAsync(userId);
        var pendingReceivedUserConnections = await this.GetPendingReceivedUserConnectionsAsync(userId);
        var pendingSentUserConnections = await this.GetPendingSentUserConnectionsAsync(userId);

        var userConnectionViewModel = new UserConnectionViewModel()
        {
            AcceptedRequests = acceptedUserConnections,
            PendingReceivedRequests = pendingReceivedUserConnections,
            PendingSentRequests = pendingSentUserConnections
        };
        
        return userConnectionViewModel;
    }

    public async Task<Friendship> GetConnectionByIdAsync(string fromUserId, string toUserId)
    => await this.timeForgeRepository.All<Friendship>()
        .Where(uc => uc.FromUserId == fromUserId && uc.ToUserId == toUserId)
        .FirstAsync();
    

    /// <summary>
    /// Validates and checks if users with this id exist in the database
    /// </summary>
    /// <param name="fromUserId">ID representing the user who is sending the request</param>
    /// <param name="toUserId">ID representing the user who will receive the request</param>
    /// <returns>true if the statement is not valid and false if it is valid</returns>
    private bool CheckConnectionExists(string fromUserId, string toUserId)
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

    /// <summary>
    /// Retrieves all accepted user connections for a user
    /// </summary>
    /// <param name="userId">String representing Identity user ID</param>
    /// <returns>
    /// List of <see cref="GetUserConnectionDto"/>
    /// </returns>
    private async Task<List<GetUserConnectionDto>> GetAcceptedUserConnectionsAsync(string userId)
    { 
        var acceptedUserConnections = await this.timeForgeRepository.All<Friendship>(uc => uc.ToUserId == userId ||
            uc.FromUserId == userId && uc.Status == FriendshipStatus.Accepted)
            .Include(uc => uc.FromUser)
            .Include(uc => uc.ToUser)
            .AsNoTracking()
            .Select(uc => new GetUserConnectionDto()
            {
                FromUserID = uc.FromUserId,
                FromUsername = uc.FromUser.UserName!,
                ToUserID = uc.ToUserId,
                ToUsername = uc.ToUser.UserName!,
                Status = uc.Status
            })
            .ToListAsync();
        
        return acceptedUserConnections;
        
    }
    /// <summary>
    /// Retrieves all pending received user connections for a user
    /// </summary>
    /// <param name="userId">String representing Identity user ID</param>
    /// <returns>
    /// List of <see cref="GetUserConnectionDto"/>
    /// </returns>
    private async Task<List<GetUserConnectionDto>> GetPendingReceivedUserConnectionsAsync(string userId)
    {
        var pendingReceivedConnections = await this.timeForgeRepository.All<Friendship>(uc => uc.ToUserId == userId 
                && uc.Status == FriendshipStatus.Pending)
            .Include(uc => uc.FromUser)
            .Include(uc => uc.ToUser)
            .AsNoTracking()
            .Select(uc => new GetUserConnectionDto()
            {
                FromUserID = uc.FromUserId,
                FromUsername = uc.FromUser.UserName!,
                ToUserID = uc.ToUserId,
                ToUsername = uc.ToUser.UserName!,
                Status = uc.Status
            })
            .ToListAsync();
        
        return pendingReceivedConnections;
    }
    /// <summary>
    /// Retrieves all pending sent user connections for a user
    /// </summary>
    /// <param name="userId">String representing Identity user ID</param>
    /// <returns>
    /// List of <see cref="GetUserConnectionDto"/>
    /// </returns>
    private async Task<List<GetUserConnectionDto>> GetPendingSentUserConnectionsAsync(string userId)
    { 
        var pendingSentUserConnections = await this.timeForgeRepository.All<Friendship>(uc => uc.FromUserId == userId &&
                uc.Status == FriendshipStatus.Pending)
            .Include(uc => uc.FromUser)
            .Include(uc => uc.ToUser)
            .AsNoTracking()
            .Select(uc => new GetUserConnectionDto()
            {
                FromUserID = uc.FromUserId,
                FromUsername = uc.FromUser.UserName!,
                ToUserID = uc.ToUserId,
                ToUsername = uc.ToUser.UserName!,
                Status = uc.Status
            })
            .ToListAsync();
        
        return pendingSentUserConnections;
    }
    
}