using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeForge.Web.Areas.Manager.Controllers;

[Area("Manager")]
public class ManagerController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}