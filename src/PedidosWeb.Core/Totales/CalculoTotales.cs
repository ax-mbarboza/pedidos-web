namespace PedidosWeb.Core.Totales;

/// <summary>Línea del pedido para el cálculo. Precio en CLP, sin decimales.</summary>
public sealed record LineaCalculo(int Cantidad, decimal PrecioUnitario, string Categoria);

/// <summary>Resultado del cálculo de totales. Todos los montos en CLP; <c>PorcDescuento</c> es una fracción (0.08).</summary>
public sealed record ResultadoTotales(
    decimal Subtotal,
    decimal BaseDescuento,
    decimal PorcDescuento,
    decimal Descuento,
    decimal Flete,
    decimal Neto,
    decimal Iva,
    decimal Total);

/// <summary>
/// Cálculo de totales del pedido. Migración fiel de <c>legado/src/UCalculoTotales.pas</c>,
/// defectos incluidos (el golden master es el contrato). Todo en <c>decimal</c>.
/// </summary>
public static class CalculoTotales
{
    private const string CategoriaOferta = "OFE";
    private const decimal RecargoVip = 0.03m;
    private const decimal TopeDescuento = 0.10m;
    private const decimal RecargoFleteRegionesExtremas = 1.15m;

    public static ResultadoTotales Calcular(
        IReadOnlyList<LineaCalculo> lineas, bool esVip, int region, decimal fleteBase, decimal tasaIva)
    {
        var subtotal = lineas.Sum(l => l.Cantidad * l.PrecioUnitario);

        // Las líneas de oferta (OFE) suman al subtotal pero no a la base del descuento.
        var baseDescuento = lineas
            .Where(l => l.Categoria.Trim().ToUpperInvariant() != CategoriaOferta)
            .Sum(l => l.Cantidad * l.PrecioUnitario);

        var porcDescuento = PorcentajeDescuento(baseDescuento, esVip);
        var descuento = Redondear(baseDescuento * porcDescuento);
        var neto = subtotal - descuento;
        var flete = lineas.Count == 0 ? 0m : CalcularFlete(fleteBase, region);
        var iva = Redondear((neto + flete) * tasaIva);
        var total = neto + flete + iva;

        return new ResultadoTotales(subtotal, baseDescuento, porcDescuento, descuento, flete, neto, iva, total);
    }

    /// <summary>
    /// Redondeo al peso, mitad hacia arriba (R05). Corrige <c>Calc3</c> del legado, que hacía
    /// <c>Trunc(valor + 0,49)</c> y bajaba el .50 exacto.
    /// </summary>
    private static decimal Redondear(decimal valor) => Math.Round(valor, MidpointRounding.AwayFromZero);

    // Umbrales estrictos (>), como UCalculoTotales.pas:77-80.
    private static decimal PorcentajeVolumen(decimal baseDescuento) =>
        baseDescuento > 1_000_000m ? 0.08m
        : baseDescuento > 500_000m ? 0.05m
        : 0m;

    private static decimal PorcentajeDescuento(decimal baseDescuento, bool esVip)
    {
        var porcentaje = PorcentajeVolumen(baseDescuento);
        if (esVip)
            porcentaje += RecargoVip;
        return Math.Min(porcentaje, TopeDescuento);
    }

    private static decimal CalcularFlete(decimal fleteBase, int region) =>
        region is 11 or 12
            ? Redondear(fleteBase * RecargoFleteRegionesExtremas)
            : Redondear(fleteBase);
}
