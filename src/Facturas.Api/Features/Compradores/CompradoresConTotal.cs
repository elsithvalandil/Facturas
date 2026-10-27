using Facturas.Api.Common.Data;
using Facturas.Api.Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Features.Compradores;

/// <summary>RF-04: compradores con el monto total de compras, de mayor a menor.</summary>
public static class CompradoresConTotal
{
    public sealed record CompradorTotalResponse(
        long RUTComprador,
        string DvComprador,
        int CantidadFacturas,
        decimal MontoTotalCompras);

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/compradores", Handle)
            .WithTags("Compradores")
            .WithName("GetCompradores")
            .WithSummary("Compradores con el monto total de compras, ordenados de mayor a menor.");

    private static Ok<IReadOnlyList<CompradorTotalResponse>> Handle(FacturaStore store) =>
        TypedResults.Ok(Calcular(store.Facturas));

    // También lo usa CompradorMayorCompra: el mayor comprador es el primero de esta lista.
    internal static IReadOnlyList<CompradorTotalResponse> Calcular(IEnumerable<Factura> facturas) =>
        facturas
            .GroupBy(f => f.Comprador)
            .Select(g => new CompradorTotalResponse(
                g.Key.Numero,
                g.Key.Dv.ToString(),
                g.Count(),
                g.Sum(f => f.Total)))
            .OrderByDescending(c => c.MontoTotalCompras)
            .ThenBy(c => c.RUTComprador)
            .ToList();
}
