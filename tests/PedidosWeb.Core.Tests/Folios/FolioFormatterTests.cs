using PedidosWeb.Core.Folios;

namespace PedidosWeb.Core.Tests.Folios;

public class FolioFormatterTests
{
    // Casos base del README (tres primeras filas de la tabla de ejemplos).
    [Theory]
    [InlineData(123, "STGO", "PED-STGO-000123")]
    [InlineData(1, "VAL", "PED-VAL-000001")]
    [InlineData(999999, "CO", "PED-CO-999999")]
    public void FormatearFolio_CasosBase_DevuelveElFolioEsperado(int numero, string sucursal, string esperado)
    {
        var folio = FolioFormatter.FormatearFolio(numero, sucursal);

        Assert.Equal(esperado, folio);
    }

    // R1: el número debe estar entre 1 y 999999.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000)]
    public void FormatearFolio_NumeroFueraDeRango_LanzaArgumentOutOfRangeException(int numero)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(numero, "STGO"));
    }

    // R2: se quitan los espacios de los extremos y se pasa a mayúsculas.
    [Theory]
    [InlineData("  stgo ", "PED-STGO-000007")]
    [InlineData("val", "PED-VAL-000007")]
    [InlineData("\tCo\n", "PED-CO-000007")]
    public void FormatearFolio_SucursalConEspaciosOMinusculas_LaNormaliza(string sucursal, string esperado)
    {
        var folio = FolioFormatter.FormatearFolio(7, sucursal);

        Assert.Equal(esperado, folio);
    }

    // R3: la sucursal debe tener de 2 a 4 letras de la A a la Z.
    [Theory]
    [InlineData("A")]
    [InlineData("ABCDE")]
    [InlineData("ÑU")]
    [InlineData("ÁB")]
    [InlineData("ST1")]
    [InlineData("S T")]
    [InlineData("  ")]
    [InlineData("")]
    public void FormatearFolio_SucursalInvalida_LanzaArgumentException(string sucursal)
    {
        Assert.Throws<ArgumentException>(() => FolioFormatter.FormatearFolio(1, sucursal));
    }

    [Fact]
    public void FormatearFolio_SucursalNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FolioFormatter.FormatearFolio(1, null!));
    }

    // R4: el número se completa con ceros a la izquierda hasta 6 dígitos.
    [Theory]
    [InlineData(5, "PED-STGO-000005")]
    [InlineData(12345, "PED-STGO-012345")]
    public void FormatearFolio_NumeroCorto_CompletaConCerosAIzquierda(int numero, string esperado)
    {
        var folio = FolioFormatter.FormatearFolio(numero, "STGO");

        Assert.Equal(esperado, folio);
    }

    // R5: si ambos son inválidos, se valida primero el número.
    [Fact]
    public void FormatearFolio_NumeroYSucursalInvalidos_ValidaPrimeroElNumero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(0, "X"));
    }
}
