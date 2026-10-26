namespace Facturas.Infrastructure.Persistence;

public sealed class FacturaDataOptions
{
    public const string SectionName = "FacturaData";

    /// <summary>Ruta al archivo JSON. Si es relativa, se resuelve contra el directorio de la aplicación.</summary>
    public string FilePath { get; set; } = "Data/Ejemplo.json";
}
