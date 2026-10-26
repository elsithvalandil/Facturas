namespace Facturas.Domain.Entities;

public sealed record DetalleFactura(int Cantidad, Producto Producto)
{
    /// <summary>Total de la línea, calculado en ejecución.</summary>
    public decimal Total => Cantidad * Producto.Valor;
}
