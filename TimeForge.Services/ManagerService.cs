using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.Infrastructure.Repositories.Interfaces;

namespace TimeForge.Services;

public class ManagerService : IManagerService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly UserManager<User> userManager;
    private readonly ILogger<ManagerService> logger;

    public ManagerService(ITimeForgeRepository timeForgeRepository,
        UserManager<User> userManager,
        ILogger<ManagerService> logger)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.timeForgeRepository = timeForgeRepository ?? throw new ArgumentNullException(nameof(timeForgeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.logger.LogInformation("Initializing ManagerService with timeForgeRepository");
    }
    
    public async Task AssignProjectToUserAsync(string projectId, string userId)
    {
        try
        {
            this.logger.LogInformation("Assigning project with ID: {ProjectId} to user with ID: {UserId}", projectId,
                userId);
            var project = await this.timeForgeRepository.GetByIdAsync<Project>(projectId);
            var user = await this.userManager.FindByIdAsync(userId);

            if (project == null || user == null)
            {
                this.logger.LogError("Project with ID: {ProjectId} or user with ID: {UserId} does not exist", projectId,
                    userId);
                throw new InvalidOperationException("Project or user does not exist");
            }

            project.AssignedUserId = userId;
            this.timeForgeRepository.Update(project);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully assigned project with ID: {ProjectId} to user with ID: {UserId}",
                projectId, userId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            this.logger.LogError("An Unexpected Error occured while attempting to assign project with ID: {ProjectId} to user with ID: {UserId} in {Service}/{Method}",
                projectId, userId, nameof(ManagerService), nameof(AssignProjectToUserAsync));
            throw;
        }
    }
}