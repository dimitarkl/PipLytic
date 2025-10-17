using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Extensions;
using server.Models;
using server.Services;

namespace server.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = HttpContext.GetUserId();
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