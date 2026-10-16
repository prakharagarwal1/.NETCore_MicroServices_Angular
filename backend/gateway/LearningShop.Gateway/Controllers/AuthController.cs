using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using LearningShop.Gateway.Models;

namespace LearningShop.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Hardcoded users for now
    private static readonly UserRecord[] Users =
    [
        new("admin",   "admin@example.com",   "Admin@123",   "Admin"),
        new("customer", "customer@example.com", "Customer@123", "Customer"),
        new("seller",  "seller@example.com",   "Seller@123",  "Seller")
    ];

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost]
    [Route("login")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { error = "Username and password are required." });
        }

        var user = Users.FirstOrDefault(u =>
            string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(u.Email, request.Username, StringComparison.OrdinalIgnoreCase));

        if (user == null || user.Password != request.Password)
        {
            _logger.LogWarning("Failed login attempt for username/email: {Username}", request.Username);
            return Unauthorized(new { error = "Invalid username/email or password." });
        }

        var token = GenerateToken(user.Username, user.Email, user.Role);
        _logger.LogInformation("User {Username} logged in successfully.", user.Username);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    private string GenerateToken(string username, string email, string role)
    {
        var key = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}