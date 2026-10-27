namespace Facturas.Api.Common.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddApiSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiKeyOptions>()
            .Bind(configuration.GetSection(ApiKeyOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && !string.IsNullOrWhiteSpace(o.HeaderName),
                "Debe configurar 'ApiKey:Key' y 'ApiKey:HeaderName'.")
            .ValidateOnStart();

        services.AddSingleton<ApiKeyEndpointFilter>();

        return services;
    }
}
