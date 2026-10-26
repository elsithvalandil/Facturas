using Facturas.Application.Abstractions;
using Facturas.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Facturas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFacturaService, FacturaService>();
        return services;
    }
}
