using Facturas.Api.Common.Domain;

namespace Facturas.Api.Common.Contracts;

// Mantiene la misma forma que la fuente JSON; TotalFactura y TotalProducto se calculan en ejecución.
// Es común porque lo devuelven cuatro slices.
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
    IReadOnlyList<DetalleFacturaDto> DetalleFactura)
{
    public static FacturaDto From(Factura factura) => new(
        factura.NumeroDocumento,
        factura.Vendedor.Numero,
        factura.Vendedor.Dv.ToString(),
        factura.Comprador.Numero,
        factura.Comprador.Dv.ToString(),
        factura.DireccionComprador,
        factura.ComunaComprador,
        factura.RegionComprador,
        factura.Total,
        factura.Detalle
            .Select(d => new DetalleFacturaDto(d.Cantidad, new ProductoDto(d.Producto.Descripcion, d.Producto.Valor), d.Total))
            .ToList());
}

public sealed record DetalleFacturaDto(int CantidadProducto, ProductoDto Producto, decimal TotalProducto);

public sealed record ProductoDto(string Descripcion, decimal Valor);
