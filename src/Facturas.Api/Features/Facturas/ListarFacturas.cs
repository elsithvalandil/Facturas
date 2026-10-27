using Facturas.Api.Common.Contracts;
using Facturas.Api.Common.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Features.Facturas;

/// <summary>RF-01: lista completa de facturas con el total calculado en ejecución.</summary>
public static class ListarFacturas
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/facturas", Handle)
            .WithTags("Facturas")
            .WithName("GetFacturas")
            .WithSummary("Lista completa de facturas con el total calculado en ejecución.");

    private static Ok<List<FacturaDto>> Handle(FacturaStore store) =>
        TypedResults.Ok(store.Facturas.Select(FacturaDto.From).ToList());
}
