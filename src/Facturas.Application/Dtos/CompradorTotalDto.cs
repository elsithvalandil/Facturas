namespace Facturas.Application.Dtos;

public sealed record CompradorTotalDto(
    long RUTComprador,
    string DvComprador,
    string Rut,
    int CantidadFacturas,
    decimal MontoTotalCompras);
