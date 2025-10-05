using Microsoft.AspNetCore.Mvc;
using server.Exceptions;
using server.Extensions;
using server.Models;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/ai-chat")]
public class AiChatController(ILogger<AiChatController> logger, IAiChatService aiChatService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var response = await aiChatService.SendMessage(userId, request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting with Gemini");
            return StatusCode(500,new {message = "An unexpected error occurred. Please try again later."} );
        }
    }

    [HttpGet]
    public IActionResult GetHistory()
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var response =  aiChatService.GetMessageHistory(userId);
            return Ok(response);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting with Gemini");
            return StatusCode(500,new {message = "An unexpected error occurred. Please try again later."} );
        }
    }
    
}