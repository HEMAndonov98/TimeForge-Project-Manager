using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.ViewModels.Task;
using TimeForge.Web.Areas.Manager.ViewModels;

namespace TimeForge.Web.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    private readonly IProjectService projectService;
    private readonly ITaskService taskService;
    private readonly UserManager<User> userManager;


    public ManagerController(IProjectService projectService,
        ITaskService taskService,
        UserManager<User> userManager)
    {
        this.projectService = projectService;
        this.taskService = taskService;
        this.userManager = userManager;
    }
    
    
    // GET
    public async Task<IActionResult> Index()
    {
        //Get Manager's ID and managed Users
       var managerId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       var managedUsers = await this.userManager
           .Users
           .Include(u => u.Projects)
           .ThenInclude(p => p.Tasks)
           .AsNoTracking()
           .Where(u => u.ManagerId == managerId)
           .Select(u => new
           {
               Username = u.UserName,
               Projects = u.Projects,
               Tasks = u.Projects.SelectMany(p => p.Tasks)
           })
           .ToListAsync();
       
       
       //Retrieve any additional data from back-end

       IEnumerable<ProjectViewModel> managerProjects = await this.projectService.GetAllProjectsAsync(managerId);
       IEnumerable<TaskViewModel> mangerTasks = managerProjects.SelectMany(p => p.Tasks);

       DateTime now = DateTime.UtcNow;
       DateTime upcoming = now.AddDays(7);
       
       var dueProjects = managedUsers.SelectMany(mu => mu.Projects
           .Where(p => p.DueDate.HasValue &&
                       p.DueDate.Value >= DateOnly.FromDateTime(now) &&
                       p.DueDate.Value <= DateOnly.FromDateTime(upcoming)
           )
           .OrderBy(p => p.DueDate)
           .Select(p => new ProjectViewModel()
           {
               Name = p.Name,
               DueDate = p.DueDate.ToString(),
           })
       );
       
       int totalTasks = mangerTasks.Count();
       int completedTasks = mangerTasks.Count(t => t.IsCompleted);
       string taskCompletionPercentage = Math.Round(
           (double)completedTasks / totalTasks, 1).ToString("P");

       int managerProjectsCount = managerProjects.Count();

       Dictionary<string, string> userTaskCompletionPrecentages = managedUsers.ToDictionary(
           u => u.Username ?? string.Empty,
           u => Math.Round(u.Tasks.Count(t => t.IsCompleted) / (double)u.Tasks.Count(), 1).ToString("P")
       );

       Dictionary<string, int> userProjectsCount = managedUsers.ToDictionary(
           u => u.Username ?? string.Empty,
           u => u.Projects.Count()
       );
        
       //Populate DashboardViewModel

       DashboardViewModel viewModel = new DashboardViewModel()
       {
           ManagerProjectsCount = managerProjectsCount,
           TaskCompletionPercentage = taskCompletionPercentage,
           DueProjects = dueProjects.ToList(),
           UsersTasksCompletionPercentage = userTaskCompletionPrecentages,
           UsersProjects = userProjectsCount
       };
       
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AssignProjects(int page = 1)
    {
        int pageSize = 12;
        int totalPages = 0;

        var managerId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var managedUsers = this.userManager.Users.Where(u => u.ManagerId == managerId);
        //Write code to also retrieve managed user projects in the future

        
        //Code for pagination
        int projectsCount = await this.projectService.GetProjectsCountAsync(managerId);
        totalPages = (int)Math.Ceiling(projectsCount / (double)pageSize);


        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;
        
        var managerProjects = await this.projectService.GetAllProjectsAsync(managerId, page, pageSize);


        ProjectAssignmentViewModel viewModel = new ProjectAssignmentViewModel()
        {
            Projects = managerProjects.ToList(),
            ManagedUsers = managedUsers.Select(mu => new SelectListItem()
            {
                Text = mu.UserName,
                Value = mu.Id
            }).ToList(),
            TotalPages = totalPages,
            CurrentPage = page
        };

        return View("AssignProjects", viewModel);
    }
}