using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Entities;
using server.Exceptions;
using server.Extensions;
using server.Models;
using server.Services;

namespace server.Controllers;

[Route("api/users/{userId}/trades")]
[ApiController]
public class TradeHistoryController(ITradeService tradeService, ILogger<TradeHistoryController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult GetUserTrades()
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var trades = tradeService.GetUserTrades(userId);

            return Ok(trades);
        }
        catch (NotFoundException ex)
        {
            return StatusCode(404, "Trades not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user trades");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPost("start")]
    public async Task<ActionResult<Trade>> StartTrade([FromBody] StartTradeDto tradeRequest)
    {
        try
        {
            var userId = HttpContext.GetUserId();

            var createdTrade = await tradeService.AddTrade(userId, tradeRequest);

            return Created($"/api/users/{userId}/trades/{createdTrade.Id}", createdTrade);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding trade into history");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPost("end")]
    public async Task<ActionResult<Trade>> EndTrade([FromBody] EndTradeDto tradeRequest)
    {
        try
        {
            var userId = HttpContext.GetUserId();

            var updatedTrade = await tradeService.EndTrade(userId, tradeRequest);

            return Ok(updatedTrade);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, "You do not have access to this trade");
        }
        catch (NotFoundException ex)
        {
            return StatusCode(404, "Trade not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding trade into history");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpGet("{tradeId}")]
    public async Task<ActionResult<Trade>> GetTrade(Guid tradeId)
    {
        try
        {
            var trade = await tradeService.GetTrade(tradeId);
            return Ok(trade);
        }
        catch (NotFoundException ex)
        {
            return StatusCode(404, "Trade not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding trade into history");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}