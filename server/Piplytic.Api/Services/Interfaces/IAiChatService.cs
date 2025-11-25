using PipLytic.Api.Models;

namespace PipLytic.Api.Services;

public interface IAiChatService
{
    Task<ChatResponse> SendMessage(Guid userId, ChatRequest request);
    List<ChatResponse> GetMessageHistory(Guid userId);
}