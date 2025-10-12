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
        try
        {
            var userId = HttpContext.GetUserId();
            var result = await marketDataService.QueryStocksData(request, userId.ToString());
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching stock data for {Symbol}", request.Symbol);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [Authorize(Roles = "Premium")]
    [EnableRateLimiting("stockRefresh")]
    [HttpPost("stocks/continue")]
    public async Task<ActionResult<string>> ContinueStockData(MarketDataDto request)
    {
        if (request.LastDate == null)
            return BadRequest(new { message = "Month field is required" });
        
        try
        {
            var userId = HttpContext.GetUserId();
            var result = await marketDataService.ContinueStocksData(request, userId.ToString());
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching stock data for {Symbol}", request.Symbol);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
    
    [Authorize]
    [EnableRateLimiting("stockRefresh")]
    [HttpPost("stocks/refresh")]
    public async Task<ActionResult<string>> RefreshStockData(MarketDataDto request)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var result = await marketDataService.RefreshStocksData(request, userId.ToString());
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching stock data for {Symbol}", request.Symbol);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}