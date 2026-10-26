namespace Facturas.Application.Dtos;

// Mantiene la misma forma que la fuente JSON; TotalFactura y TotalProducto se calculan en ejecución.
public sealed record FacturaDto(
    long NumeroDocumento,
    long RUTVendedor,
    string DvVendedor,
    long RUTComprador,
    string DvComprador,
    string DireccionComprador,
    int ComunaComprador,
    int RegionComprador,
    decimal TotalFactura,
    IReadOnlyList<DetalleFacturaDto> DetalleFactura);

public sealed record DetalleFacturaDto(int CantidadProducto, ProductoDto Producto, decimal TotalProducto);

public sealed record ProductoDto(string Descripcion, decimal Valor);
