using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnglishSandbox.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EnglishSandbox.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthSettings _settings;

    public AuthController(AuthSettings settings)
    {
        _settings = settings;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        string login = request.Login.Trim();
        string? role = _settings.ValidateCredentials(login, request.Password);

        if (role == null)
            return Unauthorized(new { message = "Неверный логин или пароль." });

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(AuthSettings.LifetimeMinutes);
        var token = new JwtSecurityToken(
            issuer: AuthSettings.Issuer,
            audience: AuthSettings.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, login),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", login),
                new Claim("role", role)
            ],
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(
                _settings.SigningKey, SecurityAlgorithms.HmacSha256));

        Response.Headers.CacheControl = "no-store";
        return Ok(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(token),
            tokenType = "Bearer",
            expiresAt = expires,
            login,
            role
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new { login = User.Identity?.Name, role = User.FindFirst("role")?.Value });
    }
}
