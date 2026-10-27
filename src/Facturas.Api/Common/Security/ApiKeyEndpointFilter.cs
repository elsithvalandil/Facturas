using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Facturas.Api.Common.Security;

/// <summary>Exige una ApiKey válida en el header configurado para todos los endpoints del grupo.</summary>
public sealed class ApiKeyEndpointFilter(IOptions<ApiKeyOptions> options) : IEndpointFilter
{
    private readonly ApiKeyOptions _options = options.Value;
    private readonly byte[] _expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(options.Value.Key));

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_options.HeaderName, out var provided) ||
            string.IsNullOrWhiteSpace(provided))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "ApiKey requerida",
                detail: $"Debe enviar la ApiKey en el header '{_options.HeaderName}'.");
        }

        // Comparación en tiempo constante sobre hashes de largo fijo.
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(provided.ToString()));
        if (!CryptographicOperations.FixedTimeEquals(providedHash, _expectedHash))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "ApiKey inválida",
                detail: "La ApiKey enviada no es válida.");
        }

        return await next(context);
    }
}
