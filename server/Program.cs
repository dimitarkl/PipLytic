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

            // Custom success-based rate limiting middleware
            app.UseMiddleware<SuccessBasedRateLimitMiddleware>();

            // Register global exception handling middleware early so it can catch downstream exceptions
            app.UseMiddleware<ExceptionMiddleware>();

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