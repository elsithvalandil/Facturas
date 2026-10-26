using Facturas.Domain.ValueObjects;

namespace Facturas.Domain.Entities;

public sealed class Factura
{
    public Factura(
        long numeroDocumento,
        Rut vendedor,
        Rut comprador,
        string direccionComprador,
        int comunaComprador,
        int regionComprador,
        IEnumerable<DetalleFactura> detalle)
    {
        NumeroDocumento = numeroDocumento;
        Vendedor = vendedor;
        Comprador = comprador;
        DireccionComprador = direccionComprador;
        ComunaComprador = comunaComprador;
        RegionComprador = regionComprador;
        Detalle = detalle.ToList().AsReadOnly();
    }

    public long NumeroDocumento { get; }
    public Rut Vendedor { get; }
    public Rut Comprador { get; }
    public string DireccionComprador { get; }
    public int ComunaComprador { get; }
    public int RegionComprador { get; }
    public IReadOnlyList<DetalleFactura> Detalle { get; }

    /// <summary>Total de la factura, calculado dinámicamente a partir del detalle.</summary>
    public decimal Total => Detalle.Sum(d => d.Total);
}
