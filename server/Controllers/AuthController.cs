using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using server.Entities;
using server.Models;
using server.Services;
using server.Exceptions;


namespace server.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<TokenResponseDto>> Register(UserDto request)
        {
            var result = await authService.RegisterAsync(request);
            var cookieOptions = authService.CreateCookieOptions();

            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            return Created("/auth/login", new { accessToken = result.AccessToken, message = "User created" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {
            var result = await authService.LoginAsync(request);

            var cookieOptions = authService.CreateCookieOptions();

            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            return Ok(new { accessToken = result.AccessToken });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Missing refresh token");

            var result = await authService.RefreshTokenAsync(refreshToken);

            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized(new { message = "Invalid refresh token" });

            var cookieOptions = authService.CreateCookieOptions();
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            return Ok(new { accessToken = result.AccessToken });
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }
    }
}