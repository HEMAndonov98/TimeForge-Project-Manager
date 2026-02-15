using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Common.Enums;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.UserConnection;

namespace TimeForge.Web.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserConnectionController : ControllerBase
{
    private readonly IConnectionService connectionService;
    private readonly ILogger<UserConnectionController> logger;

    public UserConnectionController(IConnectionService connectionService, ILogger<UserConnectionController> logger)
    {
        this.connectionService = connectionService;
        this.logger = logger;
    }
    [HttpGet]
    public async Task<ActionResult<UserConnectionViewModel>> FriendsOverview()
    {
        try
        {
            var userId = this.GetUserId();
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userConnectionsDto = await this.connectionService.GetConnectionsByUserIdAsync(userId);
            
            return Ok(userConnectionsDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting friends overview");
            return StatusCode(500, "An error occurred while retrieving friends overview.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddFriends(AddFriendInputModel inputModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? userId = this.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            inputModel.SenderId = userId;

            await this.connectionService.SendConnectionAsync(inputModel.SenderId, inputModel.Email);

            return Ok(new { message = "Friend request sent successfully." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding friend");
            return StatusCode(500, "An error occurred while sending friend request.");
        }
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptFriendRequest(string fromUserId, string toUserId)
    {
        try
        {
            var friendRequest = await this.connectionService.GetConnectionByIdAsync(fromUserId, toUserId);

            if (friendRequest == null)
            {
                return NotFound("Friend request not found.");
            }

            await this.connectionService.UpdateConnectionAsync(friendRequest, FriendshipStatus.Accepted);
            return Ok(new { message = "Friend request accepted." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accepting friend request from {From} to {To}", fromUserId, toUserId);
            return StatusCode(500, "An error occurred while accepting friend request.");
        }
    }
    
    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}