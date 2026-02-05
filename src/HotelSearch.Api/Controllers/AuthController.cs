using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HotelSearch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        // Simple validation - in production, validate against user store
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return Unauthorized(new ErrorResponse("Invalid credentials", Array.Empty<string>(), GetCorrelationId()));
        }

        // Demo: Accept any non-empty credentials, assign Admin role for "admin" user
        var isAdmin = request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);
        var token = GenerateJwtToken(request.Username, isAdmin);

        return Ok(new TokenResponse(token, _configuration.GetValue<int>("Jwt:ExpirationMinutes")));
    }

    private string GenerateJwtToken(string username, bool isAdmin)
    {
        var secretKey = _configuration["Jwt:SecretKey"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetCorrelationId()
    {
        return HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    }
}

public record TokenRequest(string Username, string Password);
public record TokenResponse(string AccessToken, int ExpiresInMinutes);
