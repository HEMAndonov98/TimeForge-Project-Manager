using Microsoft.AspNetCore.Mvc;

namespace TimeForge.Web.Controllers;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult Error404()
    {
        return View("Error404");
    }

    [Route("Error/500")]
    public IActionResult Error500()
    {
        return View("Error500");
    }

    [Route("Error/{code}")]
    public IActionResult Error(int code)
    {
        return code switch
        {
            404 => RedirectToAction("Error404"),
            500 => RedirectToAction("Error500"),
            _ => View("Error")
        };
    }
}