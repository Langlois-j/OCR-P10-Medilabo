using AuthService.Models;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthService.UnitTests;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "AuthService",
                ["Jwt:Audience"] = "Medilabo",
                ["Jwt:ExpirationMinutes"] = "15",
            })
            .Build();

        return new JwtTokenService(configuration);
    }

    [Fact]
    public void GenerateTokenContainsUsernameIssuerAndExpiration()
    {
        var service = CreateService();
        var user = new User { Username = "medilabo", PasswordHash = "hash" };

        var token = new JsonWebToken(service.GenerateToken(user));

        Assert.Equal("medilabo", token.Subject);
        Assert.Equal("AuthService", token.Issuer);
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(14));
        Assert.True(token.ValidTo <= DateTime.UtcNow.AddMinutes(15).AddSeconds(5));
    }
}
