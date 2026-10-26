using Facturas.Application.Services;
using Facturas.Domain.ValueObjects;
using Facturas.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace Facturas.Tests.Application;

// Usa el JSON real como fuente de datos.
public class FacturaServiceTests
{
    private readonly FacturaService _service = new(
        new JsonFacturaRepository(Options.Create(new FacturaDataOptions { FilePath = "Data/Ejemplo.json" })));

    [Fact]
    public async Task GetFacturas_calcula_el_total_desde_el_detalle()
    {
        var facturas = await _service.GetFacturasAsync();

        Assert.Equal(19, facturas.Count);
        Assert.All(facturas, f => Assert.Equal(f.DetalleFactura.Sum(d => d.CantidadProducto * d.Producto.Valor), f.TotalFactura));
        Assert.Equal(196_800m, facturas.Single(f => f.NumeroDocumento == 1569).TotalFactura);
    }

    [Fact]
    public async Task GetFacturasPorComprador_filtra_por_rut()
    {
        Assert.True(Rut.TryParse("21595854-K", out var rut));

        var facturas = await _service.GetFacturasPorCompradorAsync(rut);

        Assert.Equal([1570L, 1572L, 1573L, 1577L, 1585L], facturas.Select(f => f.NumeroDocumento).Order());
    }

    [Fact]
    public async Task GetCompradores_suma_totales_y_ordena_descendente()
    {
        var facturas = await _service.GetFacturasAsync();
        var compradores = await _service.GetCompradoresConTotalAsync();

        Assert.Equal(facturas.Select(f => f.RUTComprador).Distinct().Count(), compradores.Count);
        Assert.Equal(facturas.Sum(f => f.TotalFactura), compradores.Sum(c => c.MontoTotalCompras));
        Assert.Equal(compradores.OrderByDescending(c => c.MontoTotalCompras).Select(c => c.MontoTotalCompras), compradores.Select(c => c.MontoTotalCompras));

        var top = await _service.GetCompradorConMayorCompraAsync();
        Assert.Equal(compradores[0], top);
    }

    [Fact]
    public async Task Agrupacion_y_filtro_por_comuna()
    {
        var grupos = await _service.GetFacturasAgrupadasPorComunaAsync();

        Assert.Equal([12, 45, 65, 78, 89], grupos.Select(g => g.Comuna));
        Assert.Equal(19, grupos.Sum(g => g.CantidadFacturas));

        var comuna12 = await _service.GetFacturasPorComunaAsync(12);
        Assert.Equal(1577, Assert.Single(comuna12).NumeroDocumento);
        Assert.Equal(20_000m, comuna12[0].TotalFactura);

        Assert.Empty(await _service.GetFacturasPorComunaAsync(999));
    }
}
