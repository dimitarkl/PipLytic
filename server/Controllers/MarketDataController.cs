using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using server.Models;
using server.Services;
using server.Extensions;

namespace server.Controllers;

[Route("api/market")]
[ApiController]
public class MarketDataController(IMarketDataService marketDataService, ILogger<MarketDataController> logger)
    : ControllerBase
{
    [Authorize]
    [HttpPost("stocks/search")]
    public async Task<ActionResult<string>> SearchStockData(MarketDataDto request)
    {
        var userId = HttpContext.GetUserId();
        var result = await marketDataService.QueryStocksData(request, userId.ToString());
        return Ok(result);
    }

    [Authorize(Roles = "Premium")]
    [EnableRateLimiting("stockRefresh")]
    [HttpPost("stocks/continue")]
    public async Task<ActionResult<string>> ContinueStockData(MarketDataDto request)
    {
        if (request.LastDate == null)
            return BadRequest(new { message = "Month field is required" });

        var userId = HttpContext.GetUserId();
        var result = await marketDataService.ContinueStocksData(request, userId.ToString());

        return Ok(result);
    }

    [Authorize]
    [EnableRateLimiting("stockRefresh")]
    [HttpPost("stocks/refresh")]
    public async Task<ActionResult<string>> RefreshStockData(MarketDataDto request)
    {
        var userId = HttpContext.GetUserId();
        var result = await marketDataService.RefreshStocksData(request, userId.ToString());
        return Ok(result);
    }
}