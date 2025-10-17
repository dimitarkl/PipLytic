using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var userId = HttpContext.GetUserId();
        var response = await aiChatService.SendMessage(userId, request);
        return Ok(response);
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult GetHistory()
    {
        var userId = HttpContext.GetUserId();
        var response = aiChatService.GetMessageHistory(userId);
        return Ok(response);
    }
}