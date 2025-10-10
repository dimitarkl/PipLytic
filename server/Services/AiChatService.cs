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

        if (data == null) throw new DataExpiredException("Trading Data Expired Refresh The Page To Update it");

        ChatResponse response;

        var filteredData = data.Values
            .Where(v => v.DateTime < request.EndDate)
            .ToList();


        switch (user.UserType)
        {
            case EUserType.Free:
                if (!user.TryUseAi())
                    throw new QuotaExceededException("You have exceeded your free AI chat limit.");
                
                await _db.SaveChangesAsync();
                response = await SendMessageAsync(userId, request.Message, filteredData, request.EndDate);

                break;
            case EUserType.Premium:
                response = await SendMessageAsync(userId, request.Message, filteredData, request.EndDate);

                break;
            default:
                throw new Exception("User Type Not Matching With Expected types");
        }

        return response;
    }

    public List<ChatResponse> GetMessageHistory(Guid userId)
    {
        var history = _cache.GetHistory(userId);
        if (history == null) throw new NotFoundException("Chat History Not Found");

        return history.Select((content, index) => new ChatResponse
        {
            Index = index,
            Role = content.Role,
            Message = string.Join(" ", content.Parts.Select(p => p.Text))
        }).ToList();
    }

    private async Task<ChatResponse> SendMessageAsync(Guid userId, string userMessage,
        List<TimeSeriesResponseValue> filteredData, long endDate)
    {
        var history = _cache.GetHistory(userId);

        var compactDataString = SerializeToCompactString(filteredData);

        var payload = new GeminiRequestPayload
        {
            SystemInstruction = new SystemInstruction
            {
                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = _configuration["AppSettings:GeminiSystemInstructions"] +
                               $"\n\n--- FINANCIAL DATA ---\n" +
                               $"{compactDataString}\n" + // Use the compact string
                               $"The data ends at {endDate}. The user's query relates to this data."
                    }
                },
            },
            Contents = new List<Content>(history)
        };


        payload.Contents.Add(new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = userMessage } }
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
            Role = "model",
            Parts = new List<Part> { new Part { Text = reply } }
        });

        _cache.SetHistory(userId, payload.Contents);

        return new ChatResponse
        {
            Role = "model",
            Message = reply,
            Index = payload.Contents.Count
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

    private string SerializeToCompactString(List<TimeSeriesResponseValue> data)
    {
        var sb = new StringBuilder();


        var recentData = data;

        //Only For Lowering High Token Usage
        /*
            const int MaxDataPoints = 20;
             var recentData = data.Count > MaxDataPoints
                 ? data.Skip(data.Count - MaxDataPoints)
                 : data;
        */

        sb.AppendLine("OHLC Data (O,H,L,C, sorted oldest to newest. Prices rounded to 2 decimals):");

        foreach (var item in recentData)
        {
            var o = decimal.Round(item.Open, 2);
            var h = decimal.Round(item.High, 2);
            var l = decimal.Round(item.Low, 2);
            var c = decimal.Round(item.Close, 2);

            string oStr = o.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string hStr = h.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string lStr = l.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            string cStr = c.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            sb.AppendLine($"{oStr},{hStr},{lStr},{cStr}");
        }

        return sb.ToString();
    }
}