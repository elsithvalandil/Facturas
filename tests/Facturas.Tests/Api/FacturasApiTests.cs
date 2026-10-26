using System.Net;
using System.Net.Http.Json;
using Facturas.Application.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Facturas.Tests.Api;

public class FacturasApiTests(FacturasApiFactory factory) : IClassFixture<FacturasApiFactory>
{
    private const string ApiKey = "dev-api-key-7f3c2a9e4b1d";

    [Fact]
    public async Task Sin_ApiKey_responde_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/facturas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ApiKey_invalida_responde_401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "otra-key");

        var response = await client.GetAsync("/api/v1/facturas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFacturas_responde_200_con_totales_calculados()
    {
        var client = CreateClientWithApiKey();

        var facturas = await client.GetFromJsonAsync<List<FacturaDto>>("/api/v1/facturas");

        Assert.NotNull(facturas);
        Assert.Equal(19, facturas.Count);
        Assert.All(facturas, f => Assert.True(f.TotalFactura > 0));
    }

    [Theory]
    [InlineData("/api/v1/compradores", HttpStatusCode.OK)]
    [InlineData("/api/v1/compradores/mayor-compra", HttpStatusCode.OK)]
    [InlineData("/api/v1/compradores/21595854-k/facturas", HttpStatusCode.OK)]
    [InlineData("/api/v1/compradores/21595854/facturas", HttpStatusCode.OK)]
    [InlineData("/api/v1/compradores/21595854-1/facturas", HttpStatusCode.BadRequest)]
    [InlineData("/api/v1/compradores/11111111-1/facturas", HttpStatusCode.NotFound)]
    [InlineData("/api/v1/facturas/comunas", HttpStatusCode.OK)]
    [InlineData("/api/v1/facturas/comunas/45", HttpStatusCode.OK)]
    [InlineData("/api/v1/facturas/comunas/0", HttpStatusCode.BadRequest)]
    [InlineData("/api/v1/facturas/comunas/999", HttpStatusCode.NotFound)]
    public async Task Endpoints_responden_codigo_http_esperado(string url, HttpStatusCode esperado)
    {
        var client = CreateClientWithApiKey();

        var response = await client.GetAsync(url);

        Assert.Equal(esperado, response.StatusCode);
    }

    private HttpClient CreateClientWithApiKey()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
        return client;
    }
}

public sealed class FacturasApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
