using Facturas.Application.Abstractions;
using Facturas.Application.Dtos;
using Facturas.Domain.Entities;
using Facturas.Domain.ValueObjects;

namespace Facturas.Application.Services;

public sealed class FacturaService(IFacturaRepository repository) : IFacturaService
{
    public async Task<IReadOnlyList<FacturaDto>> GetFacturasAsync(CancellationToken cancellationToken = default)
    {
        var facturas = await repository.GetAllAsync(cancellationToken);
        return facturas.Select(f => f.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<FacturaDto>> GetFacturasPorCompradorAsync(Rut rut, CancellationToken cancellationToken = default)
    {
        var facturas = await repository.GetAllAsync(cancellationToken);
        return facturas
            .Where(f => f.Comprador.Numero == rut.Numero)
            .Select(f => f.ToDto())
            .ToList();
    }

    public async Task<CompradorTotalDto?> GetCompradorConMayorCompraAsync(CancellationToken cancellationToken = default)
    {
        var compradores = await GetCompradoresConTotalAsync(cancellationToken);
        return compradores.FirstOrDefault();
    }

    public async Task<IReadOnlyList<CompradorTotalDto>> GetCompradoresConTotalAsync(CancellationToken cancellationToken = default)
    {
        var facturas = await repository.GetAllAsync(cancellationToken);
        return facturas
            .GroupBy(f => f.Comprador.Numero)
            .Select(g => ToCompradorTotal(g.First().Comprador, g))
            .OrderByDescending(c => c.MontoTotalCompras)
            .ThenBy(c => c.RUTComprador)
            .ToList();
    }

    public async Task<IReadOnlyList<ComunaFacturasDto>> GetFacturasAgrupadasPorComunaAsync(CancellationToken cancellationToken = default)
    {
        var facturas = await repository.GetAllAsync(cancellationToken);
        return facturas
            .GroupBy(f => f.ComunaComprador)
            .OrderBy(g => g.Key)
            .Select(g => new ComunaFacturasDto(
                g.Key,
                g.Count(),
                g.Sum(f => f.Total),
                g.Select(f => f.ToDto()).ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<FacturaDto>> GetFacturasPorComunaAsync(int comuna, CancellationToken cancellationToken = default)
    {
        var facturas = await repository.GetAllAsync(cancellationToken);
        return facturas
            .Where(f => f.ComunaComprador == comuna)
            .Select(f => f.ToDto())
            .ToList();
    }

    private static CompradorTotalDto ToCompradorTotal(Rut comprador, IEnumerable<Factura> facturas)
    {
        var lista = facturas.ToList();
        return new CompradorTotalDto(
            comprador.Numero,
            comprador.Dv.ToString(),
            comprador.ToString(),
            lista.Count,
            lista.Sum(f => f.Total));
    }
}
