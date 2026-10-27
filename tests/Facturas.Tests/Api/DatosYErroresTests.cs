using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Facturas.Application.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Facturas.Application.Abstractions;
using Facturas.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Facturas.Tests.Api;

public class DatosYErroresTests
{
    private static JsonArray Datos() => JsonNode.Parse("""
        [{"NumeroDocumento":1,"RUTVendedor":11488103,"DvVendedor":"1",
          "RUTComprador":21595854,"DvComprador":"K","DireccionComprador":"Dirección",
          "ComunaComprador":45,"RegionComprador":13,"TotalFactura":999999,
          "DetalleFactura":[{"CantidadProducto":2,"Producto":{"Descripcion":"Producto","Valor":12.5},"TotalProducto":888888}]}]
        """)!.AsArray();

    [Theory]
    [InlineData("detalle ausente")]
    [InlineData("detalle nulo")]
    [InlineData("detalle vacío")]
    [InlineData("línea nula")]
    [InlineData("producto nulo")]
    [InlineData("cantidad negativa")]
    [InlineData("cantidad cero")]
    [InlineData("cantidad fraccionaria")]
    [InlineData("precio negativo")]
    [InlineData("precio ausente")]
    [InlineData("descripción vacía")]
    [InlineData("dirección nula")]
    [InlineData("comuna cero")]
    [InlineData("región negativa")]
    [InlineData("documento cero")]
    [InlineData("factura nula")]
    public void Datos_invalidos_impiden_arranque(string caso)
    {
        var datos = Datos();
        var factura = datos[0]!.AsObject();
        var detalle = factura["DetalleFactura"]!.AsArray();
        var linea = detalle[0]!;
        switch (caso)
        {
            case "detalle ausente": factura.Remove("DetalleFactura"); break;
            case "detalle nulo": factura["DetalleFactura"] = null; break;
            case "detalle vacío": detalle.Clear(); break;
            case "línea nula": detalle[0] = null; break;
            case "producto nulo": linea["Producto"] = null; break;
            case "cantidad negativa": linea["CantidadProducto"] = -1; break;
            case "cantidad cero": linea["CantidadProducto"] = 0; break;
            case "cantidad fraccionaria": linea["CantidadProducto"] = 1.5m; break;
            case "precio negativo": linea["Producto"]!["Valor"] = -1; break;
            case "precio ausente": linea["Producto"]!.AsObject().Remove("Valor"); break;
            case "descripción vacía": linea["Producto"]!["Descripcion"] = " "; break;
            case "dirección nula": factura["DireccionComprador"] = null; break;
            case "comuna cero": factura["ComunaComprador"] = 0; break;
            case "región negativa": factura["RegionComprador"] = -1; break;
            case "documento cero": factura["NumeroDocumento"] = 0; break;
            case "factura nula": datos[0] = null; break;
        }
        using var fuente = new FuenteTemporal(datos.ToJsonString());
        var error = Record.Exception(() => fuente.Factory.CreateClient());
        Assert.NotNull(error);
        Assert.True(error is InvalidDataException or JsonException, error.ToString());
    }

    [Fact]
    public async Task Totales_del_archivo_se_ignoran_y_empate_se_resuelve_por_rut()
    {
        var datos = Datos();
        var segunda = datos[0]!.DeepClone();
        segunda["NumeroDocumento"] = 2;
        segunda["RUTComprador"] = 11488103;
        segunda["DvComprador"] = "1";
        datos.Add(segunda);
        using var fuente = new FuenteTemporal(datos.ToJsonString());
        using var client = fuente.Client();
        var facturas = await client.GetFromJsonAsync<List<FacturaDto>>("/api/v1/facturas");
        Assert.Equal(2, facturas!.Count);
        Assert.All(facturas, f =>
        {
            Assert.Equal(25m, f.TotalFactura);
            Assert.Equal(25m, Assert.Single(f.DetalleFactura).TotalProducto);
        });
        var top = await client.GetFromJsonAsync<CompradorTotalDto>("/api/v1/compradores/mayor-compra");
        Assert.Equal(11488103, top!.RUTComprador);
        Assert.Equal(25m, top.MontoTotalCompras);
    }

    [Theory]
    [InlineData("/api/v1/facturas", 200)]
    [InlineData("/api/v1/compradores", 200)]
    [InlineData("/api/v1/facturas/comunas", 200)]
    [InlineData("/api/v1/compradores/mayor-compra", 404)]
    [InlineData("/api/v1/compradores/21595854-K/facturas", 404)]
    [InlineData("/api/v1/facturas/comunas/45", 404)]
    public async Task Fuente_vacia_respeta_contrato(string url, int status)
    {
        using var fuente = new FuenteTemporal("[]");
        using var client = fuente.Client();
        using var response = await client.GetAsync(url);
        Assert.Equal(status, (int)response.StatusCode);
        if (status == 200)
            Assert.Empty((await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
        else
            await AssertProblem(response, status);
    }

    [Theory]
    [InlineData("/api/v1/facturas", false, 401)]
    [InlineData("/api/v1/compradores/21595854-1/facturas", true, 400)]
    [InlineData("/api/v1/facturas/comunas/0", true, 400)]
    [InlineData("/api/v1/facturas/comunas/999", true, 404)]
    public async Task Errores_tienen_formato_ProblemDetails(string url, bool conKey, int status)
    {
        using var fuente = new FuenteTemporal(Datos().ToJsonString());
        using var client = conKey ? fuente.Client() : fuente.Factory.CreateClient();
        using var response = await client.GetAsync(url);
        await AssertProblem(response, status);
    }

    private static async Task AssertProblem(HttpResponseMessage response, int status)
    {
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(status, problem!.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }

    [Fact]
    public async Task Error_inesperado_responde_500_sin_exponer_detalles_internos()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ApiKey:Key", "test-key");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFacturaRepository>();
                services.AddSingleton<IFacturaRepository, RepositorioConError>();
            });
        });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Api-Key", "test-key");
        using var response = await client.GetAsync("/api/v1/facturas");
        Assert.Equal(500, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("detalle-interno-sensible", body);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(500, problem!.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
    }

    private sealed class RepositorioConError : IFacturaRepository
    {
        public Task<IReadOnlyList<Factura>> GetAllAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("detalle-interno-sensible");
    }

    private sealed class FuenteTemporal : IDisposable
    {
        private readonly string _archivo = Path.GetTempFileName();
        public WebApplicationFactory<Program> Factory { get; }

        public FuenteTemporal(string json)
        {
            File.WriteAllText(_archivo, json);
            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("FacturaData:FilePath", _archivo);
            });
        }

        public HttpClient Client()
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", "dev-api-key-7f3c2a9e4b1d");
            return client;
        }

        public void Dispose()
        {
            Factory.Dispose();
            File.Delete(_archivo);
        }
    }
}
