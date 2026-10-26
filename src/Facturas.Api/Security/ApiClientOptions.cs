namespace Facturas.Api.Security;

/// <summary>Clientes autorizados a solicitar un JWT (flujo client credentials simplificado).</summary>
public sealed class ApiClientOptions
{
    public const string SectionName = "ApiClients";

    public List<ApiClient> Clients { get; set; } = [];
}

public sealed class ApiClient
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = [];
}
