namespace Facturas.Domain.ValueObjects;

/// <summary>
/// RUT chileno (número + dígito verificador). El DV se valida con el algoritmo módulo 11.
/// </summary>
public readonly record struct Rut
{
    public long Numero { get; }
    public char Dv { get; }

    private Rut(long numero, char dv)
    {
        Numero = numero;
        Dv = dv;
    }

    public static Rut Create(long numero, string dv)
    {
        if (!EsNumeroValido(numero))
            throw new ArgumentOutOfRangeException(nameof(numero), "El RUT debe ser mayor a cero y tener como máximo nueve dígitos.");
        if (string.IsNullOrWhiteSpace(dv) || dv.Trim().Length != 1)
            throw new ArgumentException("El dígito verificador debe ser un único carácter.", nameof(dv));

        var dvNormalizado = char.ToUpperInvariant(dv.Trim()[0]);
        if (dvNormalizado != CalcularDv(numero))
            throw new ArgumentException("El dígito verificador no corresponde al número de RUT.", nameof(dv));

        return new Rut(numero, dvNormalizado);
    }

    /// <summary>
    /// Acepta "21595854-K", "21.595.854-k", "21595854K" o solo el número "21595854"
    /// (en cuyo caso el DV se calcula). Si se informa DV, debe ser válido según módulo 11.
    /// </summary>
    public static bool TryParse(string? input, out Rut rut)
    {
        rut = default;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var limpio = input.Trim().Replace(".", string.Empty).ToUpperInvariant();
        string cuerpo;
        char? dv = null;

        var guion = limpio.IndexOf('-');
        if (guion >= 0)
        {
            cuerpo = limpio[..guion];
            var resto = limpio[(guion + 1)..];
            if (resto.Length != 1)
                return false;
            dv = resto[0];
        }
        else if (limpio.EndsWith('K'))
        {
            cuerpo = limpio[..^1];
            dv = 'K';
        }
        else
        {
            cuerpo = limpio;
        }

        if (cuerpo.Length is 0 or > 9 || !cuerpo.All(char.IsAsciiDigit) || !long.TryParse(cuerpo, out var numero) || !EsNumeroValido(numero))
            return false;

        var dvCalculado = CalcularDv(numero);
        if (dv is not null && dv != dvCalculado)
            return false;

        rut = new Rut(numero, dvCalculado);
        return true;
    }

    private static bool EsNumeroValido(long numero) => numero is > 0 and <= 999_999_999;

    public static char CalcularDv(long numero)
    {
        var suma = 0L;
        var factor = 2;
        for (var n = numero; n > 0; n /= 10)
        {
            suma += n % 10 * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }

        return (11 - suma % 11) switch
        {
            11 => '0',
            10 => 'K',
            var r => (char)('0' + r)
        };
    }

    public override string ToString() => $"{Numero}-{Dv}";
}
