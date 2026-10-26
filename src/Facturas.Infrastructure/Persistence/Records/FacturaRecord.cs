namespace Facturas.Infrastructure.Persistence.Records;

// Modelos que reflejan exactamente la estructura del JSON fuente (los números vienen como "1569.0").
internal sealed record FacturaRecord(
    decimal NumeroDocumento,
    decimal RUTVendedor,
    string DvVendedor,
    decimal RUTComprador,
    string DvComprador,
    string DireccionComprador,
    decimal ComunaComprador,
    decimal RegionComprador,
    decimal TotalFactura,
    List<DetalleFacturaRecord> DetalleFactura);

internal sealed record DetalleFacturaRecord(decimal CantidadProducto, ProductoRecord Producto, decimal TotalProducto);

internal sealed record ProductoRecord(string Descripcion, decimal Valor);
