using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LearningShop.CartService.Data;
using LearningShop.CartService.Models;
using LearningShop.MessageBus.Models;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "LearningShop Cart Service API",
        Version = "v1",
        Description = "Shopping cart microservice — per-user cart management"
    });
});

builder.Services.AddDbContext<CartDbContext>(options =>
    options.UseSqlite(builder.Configuration["Database:ConnectionString"]));

// RabbitMQ + MassTransit
builder.Services.AddMessageBus(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Gateway", policy =>
    {
        policy
            .WithOrigins("http://localhost:5000", "http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart Service API v1");
    });
}

app.UseCors("Gateway");

// Trust X-User-* headers injected by the gateway (no JWT validation here)
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-User-Role", out var roles))
    {
        var claims = new List<System.Security.Claims.Claim>();
        if (context.Request.Headers.TryGetValue("X-User-Id", out var userId))
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()!));
        if (context.Request.Headers.TryGetValue("X-User-Email", out var email))
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.JwtRegisteredClaimNames.Email, email.ToString()!));
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