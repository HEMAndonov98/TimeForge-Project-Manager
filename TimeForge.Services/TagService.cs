using Microsoft.Extensions.Logging;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Services;

public class TagService : ITagService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly ILogger<TagService> logger;

    public TagService(ITimeForgeRepository timeForgeRepository, ILogger<TagService> logger)
    {
        this.timeForgeRepository = timeForgeRepository ?? throw new ArgumentNullException(nameof(timeForgeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.logger.LogInformation("TagService initialized");
    }

    public async Task CreateTagAsync(TagInputModel inputModel)
    {
        try
        {
            this.logger.LogInformation("Creating new tag with name: {TagName}", inputModel.Name);

            if (inputModel == null)
            {
                this.logger.LogError("TagInputModel is null");
                throw new ArgumentNullException(nameof(inputModel));
            }

            var newTag = new Tag()
            {
                Name = inputModel.Name,
                UserId = inputModel.UserId
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
            this.logger.LogError(e, "Error occurred while deleting tag with ID: {TagId}", tagId);
            throw;
        }
        
    }

    public async void DeleteTag(string tagId)
    {
        try
        {
            this.logger.LogInformation("Deleting tag with ID: {TagId}", tagId);

            if (string.IsNullOrEmpty(tagId))
            {
                this.logger.LogError("Tag ID is null or empty");
                throw new ArgumentNullException("Tag ID cannot be null or empty", nameof(tagId));
            }

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