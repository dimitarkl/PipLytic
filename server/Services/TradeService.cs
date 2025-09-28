using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Entities;
using server.Exceptions;
using server.Models;
using server.Utils;

namespace server.Services;

public class TradeService : ITradeService
{
    private readonly AppDbContext _db;

    public TradeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Trade> AddTrade(Guid userId, StartTradeDto request)
    {
        if (request.Type != "short" && request.Type != "long")
            throw new Exception("Invalid trade type");
        
        Trade trade = new Trade
        {
            UserId = userId,
            Type = request.Type,
            AmountInvested = request.AmountInvested,
            Symbol = request.Symbol,
            StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        _db.Add(trade);
        await _db.SaveChangesAsync();

        return trade;
    }

    public async Task<Trade> EndTrade(Guid userId, EndTradeDto request)
    {
        if (!Guid.TryParse(request.TradeId, out Guid tradeId))
            throw new Exception("Invalid trade ID format.");

        Trade trade = await _db.Trades.FindAsync(tradeId);

        if (trade == null)
            throw new NotFoundException("Trade not found");

        if (trade.UserId != userId)
            throw new ForbiddenException("You do not have access to this trade");

        trade.EndDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        trade.AmountFinal = request.AmountFinal;

        await _db.SaveChangesAsync();

        return trade;
    }

    public async Task<Trade> GetTrade(Guid tradeId)
    {
        Trade trade = await _db.Trades.FindAsync(tradeId);
        
        if (trade == null)
            throw new NotFoundException($"Trade {tradeId} not found.");

        return trade;
    }


    public async Task<List<Trade>> GetUserTrades(Guid userId)
    {
        var trades = await _db.Trades
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (!trades.Any())
            throw new NotFoundException($"Trades for user {userId} not found");

        return trades;
    }
}