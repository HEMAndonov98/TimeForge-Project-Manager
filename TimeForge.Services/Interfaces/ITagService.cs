using TimeForge.ViewModels.Tag;

namespace TimeForge.Services.Interfaces;

public interface ITagService
{
    Task CreateTagAsync(TagInputModel inputModel);

    Task<TagViewModel> GetTagByIdAsync(string tagId);

    Task<IEnumerable<TagViewModel>> GetAllTagsAsync(string userId);
    
    Task<IEnumerable<TagViewModel>> GetAllTagsByProjectIdAsync(string projectId);

    Task DeleteTagAsync(string tagId);
}