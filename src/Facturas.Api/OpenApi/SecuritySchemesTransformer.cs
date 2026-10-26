using Facturas.Api.Security;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Facturas.Api.OpenApi;

/// <summary>Documenta en OpenAPI el esquema de seguridad ApiKey (header).</summary>
internal sealed class SecuritySchemesTransformer(IOptions<ApiKeyOptions> apiKeyOptions) : IOpenApiDocumentTransformer
{
    private const string ApiKeyScheme = "ApiKey";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

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
            [new OpenApiSecuritySchemeReference(ApiKeyScheme, document)] = []
        });

        return Task.CompletedTask;
    }
}
