using Facturas.Domain.Entities;

namespace Facturas.Application.Dtos;

internal static class FacturaMappings
{
    public static FacturaDto ToDto(this Factura factura) => new(
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
