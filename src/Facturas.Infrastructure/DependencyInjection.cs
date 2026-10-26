using Facturas.Application.Abstractions;
using Facturas.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facturas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FacturaDataOptions>(configuration.GetSection(FacturaDataOptions.SectionName));
        services.AddSingleton<IFacturaRepository, JsonFacturaRepository>();
        return services;
    }
}
