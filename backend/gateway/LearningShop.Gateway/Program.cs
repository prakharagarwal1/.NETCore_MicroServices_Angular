using Microsoft.IdentityModel.Tokens;

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

// Gateway-only JWT: validate token, then inject claims as X-User-* headers
// for downstream services to trust (no per-service JWT validation needed)
app.UseAuthentication();
app.UseAuthorization();

// Inject authenticated user claims into downstream request headers
app.Use((context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                         ?? context.User.FindFirst(System.Security.Claims.JwtRegisteredClaimNames.Sub);
        var email = context.User.FindFirst(System.Security.Claims.JwtRegisteredClaimNames.Email);
        var role = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);

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