using server.Models;

namespace server.Services;

public interface IAiChatService
{
    Task<ChatResponse> SendMessage(Guid userId, ChatRequest request);
    List<ChatResponse> GetMessageHistory(Guid userId);
}