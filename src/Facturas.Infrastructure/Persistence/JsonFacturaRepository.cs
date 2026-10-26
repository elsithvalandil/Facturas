using System.Text.Json;
using Facturas.Application.Abstractions;
using Facturas.Domain.Entities;
using Facturas.Domain.ValueObjects;
using Facturas.Infrastructure.Persistence.Records;
using Microsoft.Extensions.Options;

namespace Facturas.Infrastructure.Persistence;

/// <summary>
/// Repositorio que usa el archivo JSON como única fuente de datos (simulación de base de datos).
/// El archivo se lee una sola vez y se mantiene en memoria; los totales nunca se leen del JSON.
/// </summary>
public sealed class JsonFacturaRepository : IFacturaRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly string _filePath;
    private readonly Lazy<Task<IReadOnlyList<Factura>>> _facturas;

    public JsonFacturaRepository(IOptions<FacturaDataOptions> options)
    {
        var path = options.Value.FilePath;
        _filePath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        _facturas = new Lazy<Task<IReadOnlyList<Factura>>>(LoadAsync);
    }

    public Task<IReadOnlyList<Factura>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _facturas.Value.WaitAsync(cancellationToken);

    private async Task<IReadOnlyList<Factura>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("No se encontró el archivo de datos de facturas.", _filePath);

        await using var stream = File.OpenRead(_filePath);
        var records = await JsonSerializer.DeserializeAsync<List<FacturaRecord>>(stream, SerializerOptions)
                      ?? throw new InvalidDataException("El archivo de facturas está vacío o no es válido.");

        return records.Select(ToDomain).ToList().AsReadOnly();
    }

    private static Factura ToDomain(FacturaRecord r) => new(
        ToInt64(r.NumeroDocumento),
        Rut.Create(ToInt64(r.RUTVendedor), r.DvVendedor),
        Rut.Create(ToInt64(r.RUTComprador), r.DvComprador),
        r.DireccionComprador,
        ToInt32(r.ComunaComprador),
        ToInt32(r.RegionComprador),
        (r.DetalleFactura ?? []).Select(d => new DetalleFactura(
            ToInt32(d.CantidadProducto),
            new Producto(d.Producto.Descripcion, d.Producto.Valor))));

    private static long ToInt64(decimal value) =>
        decimal.Truncate(value) == value
            ? (long)value
            : throw new InvalidDataException($"Se esperaba un valor entero y se obtuvo {value}.");

    private static int ToInt32(decimal value) => checked((int)ToInt64(value));
}
