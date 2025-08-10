using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Web.ViewComponents;

public class TagFilterWidgetViewComponent : ViewComponent
{
    private readonly ITagService tagService;

    public TagFilterWidgetViewComponent(ITagService tagService)
    {
        this.tagService = tagService;
    }
    
    public async Task<IViewComponentResult> InvokeAsync(List<string> tags)
    {
        var userId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (String.IsNullOrEmpty(userId))
        {
            return View(new List<TagViewModel>());
        }
        
        List<TagViewModel> tagList = (await this.tagService
                .GetAllTagsAsync(userId))
            .ToList();

        if (tags != null && tags.Count > 0)
        {
            foreach (var selectedTag in tags)
            {
                var checkedTag = tagList
                    .FirstOrDefault(t => t.Id == selectedTag);
                
                if (checkedTag != null)
                {
                    checkedTag.IsChecked = true;
                }
            }
        }

        return View(tagList);
    }
}