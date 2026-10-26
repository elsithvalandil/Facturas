using Facturas.Api.Security;
using Facturas.Application.Abstractions;
using Facturas.Application.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Endpoints;

public static class FacturaEndpoints
{
    public static IEndpointRouteBuilder MapFacturaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/facturas")
            .WithTags("Facturas")
            .RequireAuthorization(Policies.FacturasRead);

        group.MapGet("/", async (IFacturaService service, CancellationToken ct) =>
                TypedResults.Ok(await service.GetFacturasAsync(ct)))
            .WithName("GetFacturas")
            .WithSummary("Lista completa de facturas con el total calculado en ejecución.");

        group.MapGet("/comunas", async (IFacturaService service, CancellationToken ct) =>
                TypedResults.Ok(await service.GetFacturasAgrupadasPorComunaAsync(ct)))
            .WithName("GetFacturasAgrupadasPorComuna")
            .WithSummary("Facturas agrupadas por comuna.");

        group.MapGet("/comunas/{comuna:int}", async Task<Results<Ok<IReadOnlyList<FacturaDto>>, ProblemHttpResult>> (
                int comuna, IFacturaService service, CancellationToken ct) =>
            {
                if (comuna <= 0)
                    return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest,
                        title: "Comuna inválida", detail: "El código de comuna debe ser mayor a cero.");

                var facturas = await service.GetFacturasPorComunaAsync(comuna, ct);
                return facturas.Count == 0
                    ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound,
                        title: "Sin resultados", detail: $"No existen facturas para la comuna {comuna}.")
                    : TypedResults.Ok(facturas);
            })
            .WithName("GetFacturasPorComuna")
            .WithSummary("Facturas de una comuna específica.");

        return routes;
    }
}
