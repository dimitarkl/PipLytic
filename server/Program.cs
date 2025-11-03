using System.Text;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.Services;
using server.Middleware;

namespace server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load user secrets first in development so they're available for all configuration
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddMemoryCache();

            builder.Services.AddCors(options =>
            {
                var allowedOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:5000" }; // Fallback if config missing

                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            //Auth - Validate JWT secrets at startup
            var accessTokenSecret = builder.Configuration["AppSettings:AccessTokenSecret"]
                ?? throw new InvalidOperationException("AccessTokenSecret is not configured.");
            var refreshTokenSecret = builder.Configuration["AppSettings:RefreshTokenSecret"]
                ?? throw new InvalidOperationException("RefreshTokenSecret is not configured.");
            
            if (string.IsNullOrWhiteSpace(accessTokenSecret) || accessTokenSecret.Length < 32)
                throw new InvalidOperationException("AccessTokenSecret must be at least 32 characters long.");
            if (string.IsNullOrWhiteSpace(refreshTokenSecret) || refreshTokenSecret.Length < 32)
                throw new InvalidOperationException("RefreshTokenSecret must be at least 32 characters long.");

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
                            Encoding.UTF8.GetBytes(accessTokenSecret)),
                        ValidateIssuerSigningKey = true,
                        RoleClaimType = ClaimTypes.Role
                    };

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

            app.UseMiddleware<SuccessBasedRateLimitMiddleware>();

            app.UseMiddleware<ExceptionMiddleware>();

            app.Use(async (context, next) =>
            {
                var method = context.Request.Method;
                var path = context.Request.Path;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                // Log request without query string to avoid logging sensitive data (tokens, passwords)
                logger.LogInformation("{Method} {Path}", method, path);
                await next();
            });

            //Check if Db Exists
            DatabaseInitializer.EnsureDatabaseExists(connectionString);

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
            }

            // Seed database only if empty (runs once)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (!db.Companies.Any())
                {
                    DataSeeder.SeedTopCompanies(db);
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}