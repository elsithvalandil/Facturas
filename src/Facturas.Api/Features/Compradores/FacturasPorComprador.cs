using Facturas.Api.Common.Contracts;
using Facturas.Api.Common.Data;
using Facturas.Api.Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Facturas.Api.Features.Compradores;

/// <summary>RF-02: facturas de un comprador según su RUT.</summary>
public static class FacturasPorComprador
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/compradores/{rut}/facturas", Handle)
            .WithTags("Compradores")
            .WithName("GetFacturasPorComprador")
            .WithSummary("Facturas de un comprador según su RUT.");

    private static Results<Ok<List<FacturaDto>>, ProblemHttpResult> Handle(string rut, FacturaStore store)
    {
        if (!Rut.TryParse(rut, out var rutComprador))
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "RUT inválido",
                detail: "Formato esperado: 12345678-9, 12.345.678-K o solo el número. El DV debe ser válido.");

        var facturas = store.Facturas
            .Where(f => f.Comprador.Numero == rutComprador.Numero)
            .Select(FacturaDto.From)
            .ToList();

        return facturas.Count == 0
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Sin resultados", detail: $"No existen facturas para el comprador {rutComprador}.")
            : TypedResults.Ok(facturas);
    }
}
