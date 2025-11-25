using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PipLytic.Api.Models;
using PipLytic.Api.Services;
using PipLytic.Api.Extensions;

namespace PipLytic.Api.Controllers;

[Route("users")]
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