using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.List;

namespace TimeForge.Web.Controllers;
[Authorize]
public class TaskCollectionController(ITaskCollectionService taskCollectionService) : Controller
{
    private readonly ITaskCollectionService taskCollectionService = taskCollectionService;
    //TODO Add xml documentation when finished implementing
    //TODO add error handling
    //TODO add logging
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string userId = this.GetUserId();
        var lists = await this.taskCollectionService.GetAllTaskCollectionsAsync(userId);
        return View(lists);
    }
    
    
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TaskCollectionInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskCollectionInputModel inputModel)
    {
        inputModel.UserId = this.GetUserId();
        if (!ModelState.IsValid)
        {
            return View(inputModel);
        }

        await this.taskCollectionService.CreateTaskCollectionAsync(inputModel);
        return RedirectToAction("Index", "TaskCollection");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTaskItem(TaskItemInputModel inputModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        await this.taskCollectionService.CreateTaskItemAsync(inputModel);
        return RedirectToAction("Index", "TaskCollection");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string taskCollectionId)
    {
        try
        {
            var taskCollection = await this.taskCollectionService.GetTaskCollectionByIdAsync(taskCollectionId);
            return PartialView("Delete", taskCollection);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTaskItem(string taskId)
    {
        //TODO Implement CompletetaskItem
        // await this.taskCollectionService.CompleteTaskItem
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
}
