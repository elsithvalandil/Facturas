using Facturas.Api.Security;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/token", Results<Ok<TokenResponse>, ValidationProblem, ProblemHttpResult> (
                TokenRequest request, TokenService tokenService) =>
            {
                var errors = new Dictionary<string, string[]>();
                if (string.IsNullOrWhiteSpace(request.ClientId))
                    errors[nameof(request.ClientId)] = ["El ClientId es obligatorio."];
                if (string.IsNullOrWhiteSpace(request.ClientSecret))
                    errors[nameof(request.ClientSecret)] = ["El ClientSecret es obligatorio."];
                if (errors.Count > 0)
                    return TypedResults.ValidationProblem(errors);

                var token = tokenService.CreateToken(request);
                return token is null
                    ? TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Credenciales inválidas")
                    : TypedResults.Ok(token);
            })
            .AllowAnonymous()
            .WithName("CrearToken")
            .WithSummary("Obtiene un JWT a partir de las credenciales del cliente (requiere ApiKey).");

        return routes;
    }
}
