using AuthService.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

public class InMemoryUserService : IUserService
{
    private readonly PasswordHasher<User> _hasher = new();
    private readonly Dictionary<string, User> _users;

    public InMemoryUserService(IConfiguration configuration)
    {
        var username = configuration["Auth:DemoUser:Username"]
            ?? throw new InvalidOperationException("Auth:DemoUser:Username is missing.");
        var password = configuration["Auth:DemoUser:Password"]
            ?? throw new InvalidOperationException("Auth:DemoUser:Password is missing.");

        var hash = _hasher.HashPassword(new User { Username = username, PasswordHash = string.Empty }, password);

        _users = new Dictionary<string, User>(StringComparer.Ordinal)
        {
            [username] = new User { Username = username, PasswordHash = hash },
        };
    }

    public User? ValidateCredentials(string username, string password)
    {
        if (!_users.TryGetValue(username, out var user))
        {
            return null;
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Failed ? null : user;
    }
}
