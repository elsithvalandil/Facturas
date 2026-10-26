using Facturas.Domain.Entities;

namespace Facturas.Application.Abstractions;

public interface IFacturaRepository
{
    Task<IReadOnlyList<Factura>> GetAllAsync(CancellationToken cancellationToken = default);
}
