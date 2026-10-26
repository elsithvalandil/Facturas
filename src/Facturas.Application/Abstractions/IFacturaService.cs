using Facturas.Application.Dtos;
using Facturas.Domain.ValueObjects;

namespace Facturas.Application.Abstractions;

public interface IFacturaService
{
    Task<IReadOnlyList<FacturaDto>> GetFacturasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FacturaDto>> GetFacturasPorCompradorAsync(Rut rut, CancellationToken cancellationToken = default);

    Task<CompradorTotalDto?> GetCompradorConMayorCompraAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompradorTotalDto>> GetCompradoresConTotalAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ComunaFacturasDto>> GetFacturasAgrupadasPorComunaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FacturaDto>> GetFacturasPorComunaAsync(int comuna, CancellationToken cancellationToken = default);
}
