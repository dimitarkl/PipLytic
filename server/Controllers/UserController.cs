using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserService userService, ILogger<UserService> logger) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return Unauthorized(new { message = "User not authenticated" });

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = await userService.GetUser(userId);
        return Ok(new
        {
            user = new CurrentUserDto
            {
                Id = userId,
                Email = userData.Email,
                UserType = userData.UserType
            }
        });
    }
}