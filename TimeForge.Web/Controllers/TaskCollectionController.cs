using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.List;

namespace TimeForge.Web.Controllers;
[Authorize]
public class TaskCollectionController(ITaskCollectionService taskCollectionService) : Controller
{
    private readonly ITaskCollectionService taskCollectionService = taskCollectionService;

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TaskCollectionInputModel());
    }
}