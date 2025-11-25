using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PipLytic.Api.Entities;
using PipLytic.Api.Models;
using PipLytic.Api.Services;
using PipLytic.Api.Exceptions;
using PipLytic.Api.Extensions;

namespace PipLytic.Api.Controllers;

[Route("users/trades")]
[ApiController]
public class TradeHistoryController(ITradeService tradeService, ILogger<TradeHistoryController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Trade>>> GetUserTrades()
    {
        var userId = HttpContext.GetUserId();
        var trades = await tradeService.GetUserTrades(userId);

        return Ok(trades);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Trade>> StartTrade([FromBody] StartTradeDto tradeRequest)
    {
        var userId = HttpContext.GetUserId();

        var createdTrade = await tradeService.AddTrade(userId, tradeRequest);

        return Created($"users/trades/{createdTrade.Id}", createdTrade);
    }

    [Authorize]
    [HttpPatch]
    public async Task<ActionResult<Trade>> EndTrade([FromBody] EndTradeDto tradeRequest)
    {
        var userId = HttpContext.GetUserId();

        var updatedTrade = await tradeService.EndTrade(userId, tradeRequest);

        return Ok(updatedTrade);
    }

    [Authorize]
    [HttpGet("{tradeId}")]
    public async Task<ActionResult<Trade>> GetTrade(Guid tradeId)
    {

        var trade = await tradeService.GetTrade(tradeId);
        return Ok(trade);
    }
}