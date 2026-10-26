using Facturas.Api.Security;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Facturas.Api.OpenApi;

/// <summary>Documenta en OpenAPI los esquemas de seguridad: ApiKey (header) + JWT Bearer.</summary>
internal sealed class SecuritySchemesTransformer(IOptions<ApiKeyOptions> apiKeyOptions) : IOpenApiDocumentTransformer
{
    private const string BearerScheme = "Bearer";
    private const string ApiKeyScheme = "ApiKey";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT obtenido en POST /api/v1/auth/token"
        };
        document.Components.SecuritySchemes[ApiKeyScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = apiKeyOptions.Value.HeaderName,
            Description = "ApiKey requerida en todos los endpoints"
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(ApiKeyScheme, document)] = [],
            [new OpenApiSecuritySchemeReference(BearerScheme, document)] = []
        });

        return Task.CompletedTask;
    }
}
