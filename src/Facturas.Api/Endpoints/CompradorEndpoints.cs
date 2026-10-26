using Facturas.Application.Abstractions;
using Facturas.Application.Dtos;
using Facturas.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Endpoints;

public static class CompradorEndpoints
{
    public static IEndpointRouteBuilder MapCompradorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/compradores")
            .WithTags("Compradores");

        group.MapGet("/", async (IFacturaService service, CancellationToken ct) =>
                TypedResults.Ok(await service.GetCompradoresConTotalAsync(ct)))
            .WithName("GetCompradores")
            .WithSummary("Compradores con el monto total de compras, ordenados de mayor a menor.");

        group.MapGet("/mayor-compra", async Task<Results<Ok<CompradorTotalDto>, ProblemHttpResult>> (
                IFacturaService service, CancellationToken ct) =>
            {
                var comprador = await service.GetCompradorConMayorCompraAsync(ct);
                return comprador is null
                    ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound,
                        title: "Sin resultados", detail: "No existen facturas registradas.")
                    : TypedResults.Ok(comprador);
            })
            .WithName("GetCompradorMayorCompra")
            .WithSummary("Comprador con el mayor monto total acumulado en compras.");

        group.MapGet("/{rut}/facturas", async Task<Results<Ok<IReadOnlyList<FacturaDto>>, ProblemHttpResult>> (
                string rut, IFacturaService service, CancellationToken ct) =>
            {
                if (!Rut.TryParse(rut, out var rutComprador))
                    return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest,
                        title: "RUT inválido",
                        detail: "Formato esperado: 12345678-9, 12.345.678-K o solo el número. El DV debe ser válido.");

                var facturas = await service.GetFacturasPorCompradorAsync(rutComprador, ct);
                return facturas.Count == 0
                    ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound,
                        title: "Sin resultados", detail: $"No existen facturas para el comprador {rutComprador}.")
                    : TypedResults.Ok(facturas);
            })
            .WithName("GetFacturasPorComprador")
            .WithSummary("Facturas de un comprador según su RUT.");

        return routes;
    }
}
