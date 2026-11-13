using System.Security.Claims;
using System.Text;
using AuthService.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace AuthService.Services;

public class JwtTokenService : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();
    private readonly SigningCredentials _credentials;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _lifetime;

    public JwtTokenService(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing.");
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var minutes = int.Parse(
            configuration["Jwt:ExpirationMinutes"]
                ?? throw new InvalidOperationException("Jwt:ExpirationMinutes is missing."),
            CultureInfo.InvariantCulture);

        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        _lifetime = TimeSpan.FromMinutes(minutes);
    }

    public string GenerateToken(User user)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Username) }),
            Issuer = _issuer,
            Audience = _audience,
            Expires = DateTime.UtcNow.Add(_lifetime),
            SigningCredentials = _credentials,
        };

        return _handler.CreateToken(descriptor);
    }
}
