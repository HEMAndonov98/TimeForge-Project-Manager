using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Models;
using TimeForge.Services.Interfaces;

namespace TimeForge.Web.Controllers;
[Authorize]
public class TagController : Controller
{
    private readonly ITagService tagService;
    private readonly UserManager<User> userManager;

    public TagController(ITagService tagService,
        UserManager<User> userManager)
    {
        this.tagService = tagService;
        this.userManager = userManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? userId = this.GetUserId();
        if (String.IsNullOrEmpty(userId) || await userManager.FindByIdAsync(userId) == null)
            throw new InvalidOperationException("User not found");
        
        var tags = await this.tagService.GetAllTagsAsync(userId);
        
        return View(tags.ToList());
    }
    
    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}