using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Services;
/// <summary>
/// Service for managing tags, including creation, retrieval, and deletion operations.
/// </summary>
public class TagService : ITagService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly ILogger<TagService> logger;

    /// <summary>
/// Initializes a new instance of the <see cref="TagService"/> class.
/// </summary>
/// <param name="timeForgeRepository">The repository for data access.</param>
/// <param name="logger">The logger instance.</param>
public TagService(ITimeForgeRepository timeForgeRepository, ILogger<TagService> logger)
    {
        this.timeForgeRepository = timeForgeRepository ?? throw new ArgumentNullException(nameof(timeForgeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.logger.LogInformation("TagService initialized");
    }

    /// <summary>
/// Creates a new tag using the provided input model.
/// </summary>
/// <param name="inputModel">The tag input model.</param>
public async Task CreateTagAsync(TagInputModel inputModel)
    {
        try
        {
            if (inputModel == null)
            {
                this.logger.LogError("TagInputModel is null");
                throw new ArgumentNullException(nameof(inputModel));
            }
            
            this.logger.LogInformation("Creating new tag with name: {TagName}", inputModel.Name);


            var newTag = new Tag()
            {
                Id = inputModel.Id,
                Name = inputModel.Name,
                UserId = inputModel.UserId,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            var createdTag = await this.timeForgeRepository.AddAsync(newTag);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully created tag with ID: {TagId}", createdTag.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while creating tag: {TagName}", inputModel?.Name ?? "unknown");
            throw;
        }
    }

    /// <summary>
/// Retrieves a tag by its unique identifier.
/// </summary>
/// <param name="tagId">The tag ID.</param>
/// <returns>The tag view model.</returns>
public async Task<TagViewModel> GetTagByIdAsync(string tagId)
    {
        try
        {
            this.logger.LogInformation("Retrieving tag with ID: {tagId}", tagId);

            if (string.IsNullOrEmpty(tagId))
            {
                this.logger.LogError("Tag ID is null or empty");
                throw new ArgumentNullException("Tag ID cannot be null or empty");
            }

            var tag = await this.timeForgeRepository
                .GetByIdAsync<Tag>(tagId);

            if (tag == null)
            {
                this.logger.LogWarning("Tag with ID: {TagId} not found", tagId);
                throw new InvalidOperationException($"Tag with ID {tagId} not found");
            }

            var tagViewModel = new TagViewModel()
            {
                Name = tag.Name,
                Id = tag.Id
            };

            return tagViewModel;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error occurred while retrieving tag with ID: {TagId}", tagId);
            throw;
        }
        
    }
    
    /// <summary>
/// Retrieves all tags for a user.
/// </summary>
/// <param name="userId">The user ID.</param>
/// <returns>A collection of tag view models.</returns>
public async Task<IEnumerable<TagViewModel>> GetAllTagsAsync(string userId)
    {
        try
        {
            this.logger.LogInformation("Retrieving tag with for User with ID: {userId}", userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogError("User ID is null or empty");
                throw new ArgumentNullException("User ID cannot be null or empty");
            }

            var tags = await this.timeForgeRepository.All<Tag>(t => t.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            
            if (tags == null)
            {
                this.logger.LogWarning("An unexpected error occured while retrieving tags from database for User with ID: {userId}", userId);
                throw new ArgumentNullException("Tags cannot be null");
            }

            var tagViewModelList = tags.Select(t => new TagViewModel()
            {
                Name = t.Name,
                Id = t.Id
            });

            return tagViewModelList;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error occurred while retrieving tags with User ID: {userId}", userId);
            throw;
        }
    }

    /// <summary>
/// Retrieves all tags associated with a specific project.
/// </summary>
/// <param name="projectId">The project ID.</param>
/// <returns>A collection of tag view models.</returns>
public async Task<IEnumerable<TagViewModel>> GetAllTagsByProjectIdAsync(string projectId)
    {
        try
        {

            if (string.IsNullOrEmpty(projectId))
            {
                this.logger.LogError("User ID is null or empty");
                throw new ArgumentNullException("Project ID cannot be null or empty");
            }

            this.logger.LogInformation("Retrieving tag with for User with ID: {projectId}", projectId);


            var existingTags = await this.timeForgeRepository.All<ProjectTag>(pt => pt.ProjectId == projectId)
                .Include(pt => pt.Tag)
                .AsNoTracking()
                .OrderBy(pt => pt.Tag.Name)
                .ToListAsync();

            if (existingTags == null)
            {
                this.logger.LogWarning(
                    "An unexpected error occured while retrieving tags from database for Project with ID: {projectId}",
                    projectId);
                throw new ArgumentNullException("Tags cannot be null");
            }

            var tagViewModelList = existingTags.Select(pt => new TagViewModel()
            {
                Name = pt.Tag.Name,
                Id = pt.Tag.Id
            });

            return tagViewModelList;
        }
        catch (Exception)
        {
            this.logger.LogError("Error occurred while retrieving tags with User ID: {projectId}", projectId);
            throw;
        }
    }
    

    /// <summary>
/// Deletes a tag by its unique identifier.
/// </summary>
/// <param name="tagId">The tag ID.</param>
public async Task DeleteTagAsync(string tagId)
    {
        try
        {
            if (string.IsNullOrEmpty(tagId))
            {
                this.logger.LogError("Tag ID is null or empty");
                throw new ArgumentNullException("Tag ID cannot be null or empty", nameof(tagId));
            }
            
            this.logger.LogInformation("Deleting tag with ID: {TagId}", tagId);


            var tag = await timeForgeRepository
                .GetByIdAsync<Tag>(tagId);
            
            if (tag == null)
            {
                this.logger.LogWarning("Tag with ID: {TagId} not found", tagId);
                throw new InvalidOperationException($"Tag with ID {tagId} not found");
            }

            this.timeForgeRepository.Delete(tag);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully deleted tag with ID: {TagId}", tagId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while deleting tag with ID: {TagId}", tagId);
            throw;
        }
    }
}