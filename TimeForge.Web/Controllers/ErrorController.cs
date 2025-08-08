using Microsoft.AspNetCore.Mvc;

namespace TimeForge.Web.Controllers;

/// <summary>
/// Handles error pages and error-specific routing.
/// </summary>
public class ErrorController : Controller
{
/// <summary>
/// Returns the 404 Not Found error view.
/// </summary>
[HttpGet]
[Route("Error/404")]
public IActionResult Error404()
    {
        return View("Error404");
    }

/// <summary>
/// Returns the 500 Internal Server Error view.
/// </summary>
[HttpGet]
[Route("Error/500")]
public IActionResult Error500()
    {
        return View("Error500");
    }

/// <summary>
/// Handles generic error codes and redirects to specific error actions.
/// </summary>
/// <param name="code">The error code (e.g., 404, 500).</param>
[HttpGet]
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