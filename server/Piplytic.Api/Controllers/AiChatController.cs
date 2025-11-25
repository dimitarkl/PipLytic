using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PipLytic.Api.Models;
using PipLytic.Api.Services;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Extensions;

namespace PipLytic.Api.Controllers;

[ApiController]
[Route("ai-chat")]
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