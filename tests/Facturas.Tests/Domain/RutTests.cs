using Facturas.Domain.ValueObjects;

namespace Facturas.Tests.Domain;

public class RutTests
{
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
