using System.Globalization;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.Web.Areas.Manager.ViewModels;

namespace TimeForge.Web.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    private readonly IProjectService projectService;
    private readonly IManagerService managerService;
    private readonly UserManager<User> userManager;


    public ManagerController(IProjectService projectService,
        IManagerService managerService,
        UserManager<User> userManager)
    {
        this.managerService = managerService;
        this.projectService = projectService;
        this.userManager = userManager;
    }
    
    
    // GET
    public async Task<IActionResult> Index()
    {
        //Get Manager's ID and managed Users
       var managerId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       var managedUsers = await this.userManager
           .Users
           .Where(
               u => 
                   u.ManagerId == managerId &&
                    u.Id != managerId)
           .Select(u => new
           {
               u.Id,
               u.UserName
           })
           .AsNoTracking()
           .ToListAsync();
       
       
       //Retrieve any additional data from back-end

       var managerProjects = (await this.projectService.GetAllProjectsAsync(managerId))
           .ToArray();
       var mangerTasks = managerProjects.SelectMany(p => p.Tasks)
           .ToArray();

       DateTime now = DateTime.UtcNow.Date;
       DateTime upcoming = now.AddDays(7).Date;
       
       
       int totalTasks = mangerTasks.Count();
       int completedTasks = mangerTasks.Count(t => t.IsCompleted);
       string taskCompletionPercentage = (Math.Round(
           (double)completedTasks / totalTasks, 1)).ToString("P");
       int managerProjectsCount = managerProjects.Count();

       List<ProjectViewModel> dueProjects = new List<ProjectViewModel>();

       Dictionary<string, double> userTaskCompletionPercentages = new Dictionary<string, double>();
       Dictionary<string, int> userProjectsCount = new Dictionary<string, int>();

       foreach (var managedUser in managedUsers)
       {
           var managedUserProjects = (await this.projectService.GetAllProjectsAsync(managedUser.Id))
               .ToArray();
           var managedUserTasks = managedUserProjects
               .SelectMany(p => p.Tasks)
               .ToList();
           
           var managedUserUpcomingProjects = managedUserProjects.Where(
                   p => 
                   !String.IsNullOrEmpty(p.DueDate) &&
                    DateTime.Compare(DateTime.ParseExact(p.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), now) > 0 &&
                    DateTime.Compare(DateTime.ParseExact(p.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), upcoming) < 0)
               .ToList();
           
           var managerUpcomingProjects = managerProjects.Where(
                   p => 
                       !String.IsNullOrEmpty(p.DueDate) &&
                       DateTime.Compare(DateTime.ParseExact(p.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), now) > 0 &&
                       DateTime.Compare(DateTime.ParseExact(p.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), upcoming) < 0)
               .ToList();
           
           dueProjects.AddRange(managedUserUpcomingProjects);
           dueProjects.AddRange(managerUpcomingProjects);
           
           var managedUserCompletedTasks = managedUserTasks.Count(t => t.IsCompleted);
           var completionPercentage = managedUserTasks.Any()? Math.Round(managedUserCompletedTasks / (double)managedUserTasks.Count(), 1) * 100 : 0;
           
           userTaskCompletionPercentages[managedUser.UserName] = completionPercentage;
           userProjectsCount[managedUser.UserName] = managedUserProjects.Count();
       }

       DashboardViewModel viewModel = new DashboardViewModel()
       {
           ManagerProjectsCount = managerProjectsCount,
           ManagedUsersCount = managedUsers.Count(),
           TaskCompletionPercentage = taskCompletionPercentage,
           DueProjects = dueProjects.ToList(),
           UsersTasksCompletionPercentage = userTaskCompletionPercentages,
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
            CurrentPage = page,
        };

        return View("AssignProjects", viewModel);
    }

    public async Task<IActionResult> AssignProject(ProjectAssignInputModel inputModel)
    {

        try
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid model state");
            }

            await this.managerService.AssignProjectToUserAsync(inputModel.ProjectId, inputModel.UserId);

        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
        
        return RedirectToAction("AssignProjects", "Manager");
    }
}