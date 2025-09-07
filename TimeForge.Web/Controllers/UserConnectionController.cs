using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;

namespace TimeForge.Web.Controllers;
[Authorize]
public class UserConnectionController : Controller
{
    private readonly IConnectionService connectionService;
    
    public UserConnectionController(IConnectionService connectionService)
    {
        this.connectionService = connectionService;
    }
    [HttpGet]
    public async Task<IActionResult> AddFriends()
    {
        try
        {
            var userId = this.GetUserId();
            
            if (String.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException();
            }

            var userConnectionsDto = await this.connectionService.GetConnectionsByUserIdAsync(userId);
            
            return View(userConnectionsDto);
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}