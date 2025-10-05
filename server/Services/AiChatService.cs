using System.Text;
using System.Text.Json;
using server.Data;
using server.Entities;
using server.Enums;
using server.Exceptions;
using server.Models;
using server.Models.GeminiApi;

namespace server.Services;

public class AiChatService : IAiChatService
{
    private AppDbContext _db;
    private readonly GeminiChatCache _cache;
    private HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMarketDataService _marketDataService;

    public AiChatService(AppDbContext db, IHttpClientFactory httpClientFactory, GeminiChatCache cache,
        IConfiguration configuration, IMarketDataService marketDataService)
    {
        _configuration = configuration;
        _db = db;
        _cache = cache;
        _marketDataService = marketDataService;
        _httpClient = httpClientFactory.CreateClient("GeminiApi");
    }

    public async Task<ChatResponse> SendMessage(Guid userId, ChatRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        
        if (user == null) throw new NotFoundException("User Not Found");

        var data = _marketDataService.GetUserCacheIfExists(userId.ToString(), request.Symbol);
        
        if (data == null) throw new NotFoundException("Trading Data Expired Refresh The Page To Update it");
        

        ChatResponse response = null;


        // if (user.UserType == EUserType.Free)
        //     response = await FreeSendMessage(user, request);
        if (user.UserType == EUserType.Premium)
            response = await PremiumSendMessage(userId, request, data.Values,request.EndDate);
        
        return response;
    }

    public List<ChatResponse> GetMessageHistory(Guid userId)
    {
        var history = _cache.GetHistory(userId);
        if (history == null) throw new NotFoundException("Chat History Not Found");

        return history.Select(content => new ChatResponse
        {
            Role = content.Role,
            Message = string.Join(" ", content.Parts.Select(p => p.Text))
        }).ToList();
    }

    private async Task<ChatResponse> SendMessageAsync(Guid userId, string userMessage,
        List<TimeSeriesResponseValue> filteredData,long endDate)
    {
        var history = _cache.GetHistory(userId);

        var isFirstMessage = history.Count == 0;
        var messageText = isFirstMessage
            ? $"data: {JsonSerializer.Serialize(filteredData)}\n\n{userMessage}"
            : $"system: ignore the data after {endDate} user:{userMessage}";

        var payload = new GeminiRequestPayload
        {
            SystemInstruction = new SystemInstruction
            {
                Parts = new List<Part>
                {
                    new Part { Text = _configuration["AppSettings:GeminiSystemInstructions"] }
                },
            },
            Contents = new List<Content>(history)
        };

        payload.Contents.Add(new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = messageText } }
        });
        
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


        var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, "v1beta/models/gemini-2.5-flash-lite:generateContent")
            {
                Content = content
            };

        requestMessage.Headers.Add("x-goog-api-key", _configuration["AppSettings:GeminiApiKey"]);

        var response = await _httpClient.SendAsync(requestMessage);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Gemini Error:" + json);


        var reply = ParseReplyFromJson(json);

        payload.Contents.Add(new Content
        {
            Role = "MODEL",
            Parts = new List<Part> { new Part { Text = reply } }
        });

        _cache.SetHistory(userId, payload.Contents);

        return new ChatResponse
        {
            Role = "model",
            Message = reply,
        };
    }


    private string ParseReplyFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }

    private async Task<ChatResponse> FreeSendMessage(User user, ChatRequest request)
    {
        return null;
    }

    private async Task<ChatResponse> PremiumSendMessage(Guid userId, ChatRequest request,
        List<TimeSeriesResponseValue> filteredData,long endDate)
    {
        var response = await SendMessageAsync(userId, request.Message, filteredData,endDate);
        return response;
    }
}