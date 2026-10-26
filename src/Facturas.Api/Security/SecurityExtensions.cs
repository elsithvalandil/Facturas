using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Facturas.Api.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddApiSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiKeyOptions>()
            .Bind(configuration.GetSection(ApiKeyOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && !string.IsNullOrWhiteSpace(o.HeaderName),
                "Debe configurar 'ApiKey:Key' y 'ApiKey:HeaderName'.")
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= 32,
                "'Jwt:SigningKey' debe tener al menos 32 bytes (HS256).")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience),
                "Debe configurar 'Jwt:Issuer' y 'Jwt:Audience'.")
            .Validate(o => o.ExpirationMinutes > 0, "'Jwt:ExpirationMinutes' debe ser mayor a cero.")
            .ValidateOnStart();

        services.AddOptions<ApiClientOptions>()
            .Bind(configuration.GetSection(ApiClientOptions.SectionName))
            .Validate(o => o.Clients.Count > 0 && o.Clients.All(c =>
                    !string.IsNullOrWhiteSpace(c.ClientId) && !string.IsNullOrWhiteSpace(c.ClientSecret)),
                "Debe configurar al menos un cliente en 'ApiClients:Clients' con ClientId y ClientSecret.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<TokenService>();
        services.AddSingleton<ApiKeyEndpointFilter>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Se configura vía IOptions para que tome la configuración final (incluidos overrides en tests/entorno).
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TokenService.CreateSigningKey(jwt.SigningKey),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.FacturasRead, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(Policies.ScopeClaim, Policies.FacturasRead));

        return services;
    }
}
