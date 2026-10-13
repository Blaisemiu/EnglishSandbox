using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EnglishSandbox.Api.Auth;

public class AuthSettings
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";
    public const string Issuer = "EnglishSandbox";
    public const string Audience = "EnglishSandbox.Client";
    public const int LifetimeMinutes = 30;

    public SymmetricSecurityKey SigningKey { get; } =
        new(RandomNumberGenerator.GetBytes(32));

    private readonly PasswordHasher<string> _hasher = new();
    private readonly Dictionary<string, (string PasswordHash, string Role)> _accounts = new();

    public AuthSettings(IConfiguration configuration)
    {
        AddAccount("admin", configuration["Auth:AdminPassword"], AdminRole);
        AddAccount("user", configuration["Auth:UserPassword"], UserRole);
    }

    private void AddAccount(string login, string? password, string role)
    {
        if (!string.IsNullOrWhiteSpace(password))
        {
            _accounts[login] = (_hasher.HashPassword(login, password), role);
        }
    }

    public string? ValidateCredentials(string login, string password)
    {
        if (!_accounts.TryGetValue(login, out var account))
            return null;

        var result = _hasher.VerifyHashedPassword(login, account.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : account.Role;
    }
}
