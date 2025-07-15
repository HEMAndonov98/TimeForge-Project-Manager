using Microsoft.AspNetCore.Mvc;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Web.ViewComponents;

public class CreateTagViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string userId, string projectId)
    {
        var model = new TagInputModel() {UserId = userId, ProjectId = projectId};
        return View(model);
    }
}