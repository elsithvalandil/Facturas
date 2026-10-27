using Facturas.Api.Common.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using static Facturas.Api.Features.Compradores.CompradoresConTotal;

namespace Facturas.Api.Features.Compradores;

/// <summary>RF-03: comprador con el mayor monto total acumulado en compras.</summary>
public static class CompradorMayorCompra
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/compradores/mayor-compra", Handle)
            .WithTags("Compradores")
            .WithName("GetCompradorMayorCompra")
            .WithSummary("Comprador con el mayor monto total acumulado en compras.");

    private static Results<Ok<CompradorTotalResponse>, ProblemHttpResult> Handle(FacturaStore store)
    {
        var comprador = Calcular(store.Facturas).FirstOrDefault();
        return comprador is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Sin resultados", detail: "No existen facturas registradas.")
            : TypedResults.Ok(comprador);
    }
}
