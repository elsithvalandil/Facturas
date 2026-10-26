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

    private readonly IReadOnlyList<Factura> _facturas;

    public JsonFacturaRepository(IOptions<FacturaDataOptions> options)
    {
        var path = options.Value.FilePath;
        var filePath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        _facturas = Load(filePath);
    }

    // La interfaz es asíncrona porque representa una base de datos; aquí los datos ya están en memoria.
    public Task<IReadOnlyList<Factura>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_facturas);

    private static IReadOnlyList<Factura> Load(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var records = JsonSerializer.Deserialize<List<FacturaRecord>>(stream, SerializerOptions)
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
