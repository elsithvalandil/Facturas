using System.Net.Http.Json;
using Facturas.Api.Common.Contracts;
using Facturas.Api.Features.Compradores;
using Facturas.Api.Features.Facturas;
using Facturas.Tests.Api;

namespace Facturas.Tests.Features;

// Cada slice se prueba a través de su endpoint, usando el JSON real como fuente de datos.
public class FeaturesTests(FacturasApiFactory factory) : IClassFixture<FacturasApiFactory>
{
    private readonly HttpClient _client = CreateClient(factory);

    [Fact]
    public async Task ListarFacturas_calcula_el_total_desde_el_detalle()
    {
        var facturas = await _client.GetFromJsonAsync<List<FacturaDto>>("/api/v1/facturas");

        Assert.NotNull(facturas);
        Assert.Equal(19, facturas.Count);
        Assert.All(facturas, f => Assert.Equal(f.DetalleFactura.Sum(d => d.CantidadProducto * d.Producto.Valor), f.TotalFactura));
        Assert.Equal(196_800m, facturas.Single(f => f.NumeroDocumento == 1569).TotalFactura);
    }

    [Fact]
    public async Task FacturasPorComprador_filtra_por_rut()
    {
        var facturas = await _client.GetFromJsonAsync<List<FacturaDto>>("/api/v1/compradores/21595854-K/facturas");

        Assert.NotNull(facturas);
        Assert.Equal([1570L, 1572L, 1573L, 1577L, 1585L], facturas.Select(f => f.NumeroDocumento).Order());
    }

    [Fact]
    public async Task CompradoresConTotal_suma_totales_y_ordena_descendente()
    {
        var facturas = await _client.GetFromJsonAsync<List<FacturaDto>>("/api/v1/facturas");
        var compradores = await _client.GetFromJsonAsync<List<CompradoresConTotal.CompradorTotalResponse>>("/api/v1/compradores");

        Assert.NotNull(facturas);
        Assert.NotNull(compradores);
        Assert.Equal(facturas.Select(f => f.RUTComprador).Distinct().Count(), compradores.Count);
        Assert.Equal(facturas.Sum(f => f.TotalFactura), compradores.Sum(c => c.MontoTotalCompras));
        Assert.Equal(compradores.OrderByDescending(c => c.MontoTotalCompras).Select(c => c.MontoTotalCompras), compradores.Select(c => c.MontoTotalCompras));

        var top = await _client.GetFromJsonAsync<CompradoresConTotal.CompradorTotalResponse>("/api/v1/compradores/mayor-compra");
        Assert.Equal(compradores[0], top);
    }

    [Fact]
    public async Task Agrupacion_y_filtro_por_comuna()
    {
        var grupos = await _client.GetFromJsonAsync<List<FacturasAgrupadasPorComuna.ComunaFacturasResponse>>("/api/v1/facturas/comunas");

        Assert.NotNull(grupos);
        Assert.Equal([12, 45, 65, 78, 89], grupos.Select(g => g.Comuna));
        Assert.Equal(19, grupos.Sum(g => g.CantidadFacturas));

        var comuna12 = await _client.GetFromJsonAsync<List<FacturaDto>>("/api/v1/facturas/comunas/12");
        Assert.NotNull(comuna12);
        Assert.Equal(1577, Assert.Single(comuna12).NumeroDocumento);
        Assert.Equal(20_000m, comuna12[0].TotalFactura);
    }

    private static HttpClient CreateClient(FacturasApiFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "dev-api-key-7f3c2a9e4b1d");
        return client;
    }
}
