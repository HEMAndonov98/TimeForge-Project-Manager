using TimeForge.ViewModels.Tag;

namespace TimeForge.Services.Interfaces;

public interface ITagService
{
    Task CreateTagAsync(TagInputModel inputModel);

    Task<TagViewModel> GetTagByIdAsync(string tagId);

    void DeleteTag(string tagId);
}