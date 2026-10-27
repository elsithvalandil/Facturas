using Facturas.Domain.ValueObjects;

namespace Facturas.Tests.Domain;

public class RutTests
{
    [Theory]
    [InlineData(21595854, "k")]
    [InlineData(11488103, "1")]
    [InlineData(10782109, "0")]
    public void Create_y_TryParse_producen_el_mismo_rut(long numero, string dv)
    {
        Assert.True(Rut.TryParse($"{numero}-{dv}", out var esperado));
        Assert.Equal(esperado, Rut.Create(numero, dv));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("X")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("KK")]
    public void Create_rechaza_dv_invalido(string? dv)
    {
        Assert.Throws<ArgumentException>(() => Rut.Create(21595854, dv!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000000)]
    public void Create_y_TryParse_rechazan_numero_fuera_de_rango(long numero)
    {
        var dv = Rut.CalcularDv(numero).ToString();
        Assert.Throws<ArgumentOutOfRangeException>(() => Rut.Create(numero, dv));
        Assert.False(Rut.TryParse($"{numero}-{dv}", out _));
    }

    [Theory]
    [InlineData("21595854-k", 21595854, 'K')]
    [InlineData("21.595.854-K", 21595854, 'K')]
    [InlineData("21595854K", 21595854, 'K')]
    [InlineData("21595854", 21595854, 'K')]
    [InlineData("11488103-1", 11488103, '1')]
    [InlineData("10782109-0", 10782109, '0')]
    public void TryParse_formatos_validos(string input, long numero, char dv)
    {
        Assert.True(Rut.TryParse(input, out var rut));
        Assert.Equal(numero, rut.Numero);
        Assert.Equal(dv, rut.Dv);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("21595854-1")]
    [InlineData("21595854-")]
    [InlineData("-K")]
    [InlineData("0")]
    public void TryParse_rechaza_entradas_invalidas(string? input)
    {
        Assert.False(Rut.TryParse(input, out _));
    }
}
