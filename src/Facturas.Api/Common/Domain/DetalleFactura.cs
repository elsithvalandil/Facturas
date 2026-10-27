namespace Facturas.Api.Common.Domain;

public sealed record DetalleFactura(int Cantidad, Producto Producto)
{
    /// <summary>Total de la línea, calculado en ejecución.</summary>
    public decimal Total => Cantidad * Producto.Valor;
}
