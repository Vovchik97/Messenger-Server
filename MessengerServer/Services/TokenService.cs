using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MessengerServer.Services;

public class TokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;
    private readonly int _expiryMinutes;

    public TokenService(IConfiguration config)
    {
        _key = config["Jwt:Key"] ?? "fallback_key_must_be_32_chars_or_more!";
        _issuer = config["Jwt:Issuer"] ?? "MessengerApi";
        _audience = config["Jwt:Audience"] ?? "MessengerClients";
        _expiryMinutes = 10080;
    }

    public string GenerateToken(int userId, string username)
    {
        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}