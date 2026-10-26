namespace Facturas.Application.Dtos;

public sealed record CompradorTotalDto(
    long RUTComprador,
    string DvComprador,
    int CantidadFacturas,
    decimal MontoTotalCompras);
