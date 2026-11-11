using System.Globalization;
using PedidosWeb.Core.Totales;

namespace PedidosWeb.Core.Tests.Totales;

/// <summary>
/// Compara el cálculo de totales en C# contra el golden master del legado
/// (<c>migracion/golden-master.csv</c>): descuento, flete, IVA y total.
/// </summary>
public class CalculoTotalesTests
{
    private const string RutaGoldenMaster = "migracion/golden-master.csv";

    public static IEnumerable<object[]> CasosGoldenMaster()
    {
        var lineasCsv = File.ReadAllLines(BuscarGoldenMaster()).Skip(1);

        foreach (var linea in lineasCsv.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            // Ninguna columna lleva comas: las líneas del pedido se separan con '|'.
            var c = linea.Split(',');
            yield return new object[]
            {
                int.Parse(c[0], CultureInfo.InvariantCulture),      // caso
                c[2],                                               // tipo_cliente
                int.Parse(c[3], CultureInfo.InvariantCulture),      // region
                Dec(c[4]),                                          // flete_base
                Dec(c[5]),                                          // tasa_iva
                c[6],                                               // lineas
                Dec(c[10]),                                         // descuento
                Dec(c[11]),                                         // flete
                Dec(c[13]),                                         // iva
                Dec(c[14]),                                         // total
            };
        }
    }

    [Theory]
    [MemberData(nameof(CasosGoldenMaster))]
    public void Calcular_CasoDelGoldenMaster_CoincideConElLegado(
        int caso, string tipoCliente, int region, decimal fleteBase, decimal tasaIva,
        string lineas, decimal descuentoEsperado, decimal fleteEsperado,
        decimal ivaEsperado, decimal totalEsperado)
    {
        var resultado = CalculoTotales.Calcular(
            ParsearLineas(lineas), tipoCliente == "V", region, fleteBase, tasaIva);

        Assert.True(descuentoEsperado == resultado.Descuento, $"Caso {caso}: descuento");
        Assert.True(fleteEsperado == resultado.Flete, $"Caso {caso}: flete");
        Assert.True(ivaEsperado == resultado.Iva, $"Caso {caso}: IVA");
        Assert.True(totalEsperado == resultado.Total, $"Caso {caso}: total");
    }

    // ---- Casos de borde que el golden master no cubre ----

    [Fact]
    public void Calcular_SinLineas_TodoEnCeroIncluidoFlete()
    {
        var r = CalculoTotales.Calcular(new List<LineaCalculo>(), true, 12, 16490m, 0.19m);

        Assert.Equal(0m, r.Subtotal);
        Assert.Equal(0m, r.Descuento);
        Assert.Equal(0m, r.Flete);
        Assert.Equal(0m, r.Iva);
        Assert.Equal(0m, r.Total);
    }

    [Theory]
    [InlineData(500_001, 0.05, 25_000, 90_915, 569_416)]
    [InlineData(1_000_001, 0.08, 80_000, 175_465, 1_098_966)]
    public void Calcular_BaseUnPesoSobreElUmbral_AplicaTramoSuperior(
        int monto, double porcentaje, int descuento, int iva, int total)
    {
        var r = CalculoTotales.Calcular(
            new List<LineaCalculo> { new(1, monto, "ABA") }, false, 13, 3500m, 0.19m);

        Assert.Equal((decimal)porcentaje, r.PorcDescuento);
        Assert.Equal(descuento, r.Descuento);
        Assert.Equal(iva, r.Iva);
        Assert.Equal(total, r.Total);
    }

    [Fact]
    public void Calcular_CategoriaOfertaConEspaciosOMinusculas_ExcluyeDeLaBase()
    {
        var lineas = new List<LineaCalculo>
        {
            new(1, 600_000m, " OFE "),
            new(1, 600_000m, "ofe"),
            new(1, 400_000m, "ABA"),
        };

        var r = CalculoTotales.Calcular(lineas, false, 13, 3500m, 0.19m);

        Assert.Equal(1_600_000m, r.Subtotal);
        Assert.Equal(400_000m, r.BaseDescuento);
        Assert.Equal(0m, r.Descuento);
        Assert.Equal(304_665m, r.Iva);
        Assert.Equal(1_908_165m, r.Total);
    }

    [Fact]
    public void Calcular_VipConSoloOfertas_PorcentajeTresPorCientoDescuentoCero()
    {
        var r = CalculoTotales.Calcular(
            new List<LineaCalculo> { new(3, 10_000m, "OFE") }, true, 13, 3500m, 0.19m);

        Assert.Equal(0.03m, r.PorcDescuento);
        Assert.Equal(0m, r.Descuento);
        Assert.Equal(30_000m, r.Neto);
        Assert.Equal(6_365m, r.Iva);
        Assert.Equal(39_865m, r.Total);
    }

    [Fact]
    public void Calcular_LineaConCantidadCero_CobraFleteAunqueSubtotalSeaCero()
    {
        var r = CalculoTotales.Calcular(
            new List<LineaCalculo> { new(0, 5_000m, "ABA") }, false, 12, 16490m, 0.19m);

        Assert.Equal(0m, r.Subtotal);
        Assert.Equal(18_964m, r.Flete);
        Assert.Equal(3_603m, r.Iva);
        Assert.Equal(22_567m, r.Total);
    }

    private static decimal Dec(string valor) => decimal.Parse(valor, CultureInfo.InvariantCulture);

    /// <summary>Formato <c>cantidad x precio x categoría</c>, separadas por '|'.</summary>
    private static List<LineaCalculo> ParsearLineas(string lineas) =>
        lineas
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Split('x'))
            .Select(p => new LineaCalculo(
                int.Parse(p[0], CultureInfo.InvariantCulture), Dec(p[1]), p[2]))
            .ToList();

    /// <summary>Sube desde la carpeta del test hasta encontrar la raíz que tiene el golden master.</summary>
    private static string BuscarGoldenMaster()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var ruta = Path.Combine(dir.FullName, RutaGoldenMaster);
            if (File.Exists(ruta))
                return ruta;
        }

        throw new FileNotFoundException($"No se encontró {RutaGoldenMaster} subiendo desde {AppContext.BaseDirectory}.");
    }
}
