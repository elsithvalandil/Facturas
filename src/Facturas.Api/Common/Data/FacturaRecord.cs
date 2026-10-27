namespace Facturas.Api.Common.Data;

// Modelos de lectura del JSON fuente (los números vienen como "1569.0", por eso decimal).
// TotalFactura y TotalProducto existen en el JSON pero no se mapean: los totales se calculan en el dominio.
internal sealed record FacturaRecord(
    decimal NumeroDocumento,
    decimal RUTVendedor,
    string DvVendedor,
    decimal RUTComprador,
    string DvComprador,
    string DireccionComprador,
    decimal ComunaComprador,
    decimal RegionComprador,
    List<DetalleFacturaRecord> DetalleFactura);

internal sealed record DetalleFacturaRecord(decimal CantidadProducto, ProductoRecord Producto);

internal sealed record ProductoRecord(string Descripcion, decimal Valor);
