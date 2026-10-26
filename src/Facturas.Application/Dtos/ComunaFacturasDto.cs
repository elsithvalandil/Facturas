namespace Facturas.Application.Dtos;

public sealed record ComunaFacturasDto(
    int Comuna,
    int CantidadFacturas,
    decimal MontoTotal,
    IReadOnlyList<FacturaDto> Facturas);
