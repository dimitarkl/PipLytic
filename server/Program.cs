using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.Services;

namespace server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddMemoryCache();

            // Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("stockRefresh", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 1;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        message = "Too many requests. Please try again later.",
                        retryAfter = "1 minute"
                    }, cancellationToken);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var endpoint = context.GetEndpoint();
                    var hasStockRefreshPolicy =
                        endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == "stockRefresh";

                    if (!hasStockRefreshPolicy)
                    {
                        return RateLimitPartition.GetNoLimiter<string>("global");
                    }

                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5000") // your React dev server
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // important for cookies
                });
            });

            //Auth
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["AppSettings:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
                        ValidateIssuerSigningKey = true,
                        RoleClaimType = ClaimTypes.Role
                    };

                    // Clear default claim type mappings to avoid transformation issues
                    options.MapInboundClaims = false;
                });

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IMarketDataService, MarketDataService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITradeService, TradeService>();
            builder.Services.AddScoped<IAiChatService, AiChatService>();
            builder.Services.AddScoped<ICompaniesService, CompaniesService>();

            builder.Services.AddScoped<GeminiChatCache>(provider =>
                new GeminiChatCache(
                    provider.GetRequiredService<IMemoryCache>(),
                    TimeSpan.FromMinutes(5)
                )
            );

            builder.Services.AddHttpClient("TwelveData", client =>
            {
                client.BaseAddress = new Uri("https://api.twelvedata.com/");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddHttpClient("GeminiApi", client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            var app = builder.Build();

            app.UseCors("AllowReactApp");

            app.UseRateLimiter();

            // Log every HTTP request
            app.Use(async (context, next) =>
            {
                var method = context.Request.Method;
                var path = context.Request.Path;
                var query = context.Request.QueryString;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("{Method} {Path}{Query}", method, path, query);
                await next();
            });

            //Check if Db Exists
            DatabaseInitializer.EnsureDatabaseExists(builder.Configuration.GetConnectionString("DefaultConnection"));

            //Migrations
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                builder.Configuration.AddUserSecrets<Program>();
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                DataSeeder.SeedTopCompanies(db);
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}