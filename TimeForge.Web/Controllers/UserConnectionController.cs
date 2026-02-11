using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Common.Enums;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.UserConnection;

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
    public async Task<IActionResult> FriendsOverview()
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
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFriends(AddFriendInputModel inputModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            string? userId = this.GetUserId();

            if (String.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("User ID is null or empty");
            }

            inputModel.SenderId = userId;

            await this.connectionService.SendConnectionAsync(inputModel.SenderId, inputModel.Email);

            return RedirectToAction("FriendsOverview", "UserConnection");
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptFriendRequest(string fromUserId, string toUserId)
    {
        var friendRequest = await this.connectionService.GetConnectionByIdAsync(fromUserId, toUserId);

        await this.connectionService.UpdateConnectionAsync(friendRequest, ConnectionStatus.Accepted);
        return RedirectToAction("FriendsOverview", "UserConnection");
    }
    
    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}