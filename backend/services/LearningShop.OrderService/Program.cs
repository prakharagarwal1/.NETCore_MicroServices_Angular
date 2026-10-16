using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LearningShop.MessageBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "LearningShop Order Service API",
        Version = "v1",
        Description = "Order microservice — order management"
    });
});

// RabbitMQ + MassTransit
builder.Services.AddMessageBus(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
    });
}

// Trust X-User-* headers injected by the gateway (no JWT validation here)
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-User-Role", out var roles))
    {
        var claims = new List<System.Security.Claims.Claim>();
        if (context.Request.Headers.TryGetValue("X-User-Id", out var userId))
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()!));
        if (context.Request.Headers.TryGetValue("X-User-Email", out var email))
            claims.Add(new System.Security.Claims.Claim("email", email.ToString()!));
        foreach (var r in roles.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries))
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r.Trim()));

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "GatewayHeaders");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    }
    return next(context);
});

app.UseAuthorization();
app.MapControllers();

app.Run();