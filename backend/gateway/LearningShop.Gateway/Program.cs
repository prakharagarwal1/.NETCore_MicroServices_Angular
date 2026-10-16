using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "LearningShop Gateway API",
        Version = "v1",
        Description = "API Gateway with JWT Authentication and Reverse Proxy"
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy")
    );

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT Authentication — validated ONCE at the gateway
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// API Key settings
builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection("ApiKey"));

var app = builder.Build();

app.UseCors("Angular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LearningShop Gateway API v1");
    });
}

// Simple API key middleware — checks for X-Api-Key header
// If present and valid, sets user claims. If not, lets JWT auth handle it.
app.Use((context, next) =>
{
    var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (!string.IsNullOrEmpty(apiKey))
    {
        // Validate against configured API keys
        var apiKeySettings = context.RequestServices.GetService<ApiKeySettings>();
        if (apiKeySettings != null && apiKeySettings.ValidKeys.Contains(apiKey))
        {
            var claims = new[]
            {
                new Claim("ApiKeyScheme", "ApiKey"),
                new Claim(ClaimTypes.NameIdentifier, apiKey)
            };
            var identity = new ClaimsIdentity(claims, "ApiKey");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;
        }
    }
    // If no valid API key, let JWT auth (added later) handle it
    return next(context);
});

// Inject authenticated user claims into downstream request headers
app.Use((context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)
                         ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);
        var email = context.User.FindFirst(JwtRegisteredClaimNames.Email);
        var role = context.User.FindFirst(ClaimTypes.Role);

        if (userId != null)
            context.Request.Headers["X-User-Id"] = userId.Value;
        if (email != null)
            context.Request.Headers["X-User-Email"] = email.Value;
        if (role != null)
            context.Request.Headers["X-User-Role"] = role.Value;
    }
    return next(context);
});

app.MapControllers();
app.MapReverseProxy();

app.Run();

public class ApiKeySettings
{
    public List<string> ValidKeys { get; set; } = new();
}