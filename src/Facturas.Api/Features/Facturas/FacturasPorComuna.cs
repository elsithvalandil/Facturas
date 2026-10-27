using Facturas.Api.Common.Contracts;
using Facturas.Api.Common.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Features.Facturas;

/// <summary>RF-06: facturas de una comuna específica.</summary>
public static class FacturasPorComuna
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/facturas/comunas/{comuna:int}", Handle)
            .WithTags("Facturas")
            .WithName("GetFacturasPorComuna")
            .WithSummary("Facturas de una comuna específica.");

    private static Results<Ok<List<FacturaDto>>, ProblemHttpResult> Handle(int comuna, FacturaStore store)
    {
        if (comuna <= 0)
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Comuna inválida", detail: "El código de comuna debe ser mayor a cero.");

        var facturas = store.Facturas
            .Where(f => f.ComunaComprador == comuna)
            .Select(FacturaDto.From)
            .ToList();

        return facturas.Count == 0
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Sin resultados", detail: $"No existen facturas para la comuna {comuna}.")
            : TypedResults.Ok(facturas);
    }
}
