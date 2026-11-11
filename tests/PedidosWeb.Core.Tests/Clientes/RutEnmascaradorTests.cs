using PedidosWeb.Core.Clientes;

namespace PedidosWeb.Core.Tests.Clientes;

public class RutEnmascaradorTests
{
    [Theory]
    [InlineData("76.123.456-0", "**.***.456-0")] // con puntos
    [InlineData("77654321-7", "**.***.321-7")] // sin puntos
    [InlineData("76 432 198-7", "**.***.198-7")] // con espacios
    [InlineData("76.990.011-k", "**.***.011-K")] // con la «k» en minúscula
    [InlineData("76123456-0", "**.***.456-0")] // ya normalizado, como lo entrega ObtenerFichaTexto
    public void Enmascarar_RutValido_DejaLosUltimosTresDigitosYElVerificador(string rut, string esperado)
    {
        var enmascarado = RutEnmascarador.Enmascarar(rut);

        Assert.Equal(esperado, enmascarado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sin-formato")]
    [InlineData("76123456")] // sin dígito verificador
    [InlineData("12-3")] // cuerpo demasiado corto
    public void Enmascarar_RutSinFormatoEsperado_LoOcultaPorCompleto(string? rut)
    {
        var enmascarado = RutEnmascarador.Enmascarar(rut);

        Assert.Equal("**.***.***-*", enmascarado);
    }
}
