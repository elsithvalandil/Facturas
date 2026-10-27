using Facturas.Api.Common.Contracts;
using Facturas.Api.Common.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Features.Facturas;

/// <summary>RF-05: facturas agrupadas por comuna del comprador.</summary>
public static class FacturasAgrupadasPorComuna
{
    public sealed record ComunaFacturasResponse(
        int Comuna,
        int CantidadFacturas,
        decimal MontoTotal,
        IReadOnlyList<FacturaDto> Facturas);

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/facturas/comunas", Handle)
            .WithTags("Facturas")
            .WithName("GetFacturasAgrupadasPorComuna")
            .WithSummary("Facturas agrupadas por comuna.");

    private static Ok<List<ComunaFacturasResponse>> Handle(FacturaStore store) =>
        TypedResults.Ok(store.Facturas
            .GroupBy(f => f.ComunaComprador)
            .OrderBy(g => g.Key)
            .Select(g => new ComunaFacturasResponse(
                g.Key,
                g.Count(),
                g.Sum(f => f.Total),
                g.Select(FacturaDto.From).ToList()))
            .ToList());
}
