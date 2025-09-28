using server.Entities;
using server.Models;

namespace server.Services;

public interface ITradeService
{
    Task<Trade> AddTrade(Guid userId, StartTradeDto trade);
    Task<Trade> GetTrade(Guid tradeId);
    Task<Trade> EndTrade(Guid userId, EndTradeDto request);
    Task<List<Trade>> GetUserTrades(Guid userId);
}