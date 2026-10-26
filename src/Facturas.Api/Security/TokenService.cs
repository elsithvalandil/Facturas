using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Facturas.Api.Security;

public sealed record TokenRequest(string ClientId, string ClientSecret);

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);

public sealed class TokenService(IOptions<JwtOptions> jwtOptions, IOptions<ApiClientOptions> clientOptions, TimeProvider timeProvider)
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public TokenResponse? CreateToken(TokenRequest request)
    {
        var client = clientOptions.Value.Clients.FirstOrDefault(c =>
            string.Equals(c.ClientId, request.ClientId, StringComparison.Ordinal));

        if (client is null || !SecretMatches(client.ClientSecret, request.ClientSecret))
            return null;

        var now = timeProvider.GetUtcNow();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.ClientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(client.Scopes.Select(s => new Claim(Policies.ScopeClaim, s)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = now.AddMinutes(_jwt.ExpirationMinutes).UtcDateTime,
            SigningCredentials = new SigningCredentials(CreateSigningKey(_jwt.SigningKey), SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResponse(token, "Bearer", _jwt.ExpirationMinutes * 60);
    }

    public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));

    private static bool SecretMatches(string expected, string provided) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
            SHA256.HashData(Encoding.UTF8.GetBytes(provided ?? string.Empty)));
}
