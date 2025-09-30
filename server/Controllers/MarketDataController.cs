using System.Globalization;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;
using Microsoft.Extensions.Logging;


namespace server.Controllers;

[Route("api/market")]
[ApiController]
public class MarketDataController(IMarketDataService marketDataService, ILogger<MarketDataController> logger)
    : ControllerBase
{
    [HttpPost("stocks/search")]
    public async Task<ActionResult<string>> SearchStockData(MarketDataDto request)
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var result = await marketDataService.QueryStocksData(request, userIdClaim);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching stock data for {Symbol}", request.Symbol);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}