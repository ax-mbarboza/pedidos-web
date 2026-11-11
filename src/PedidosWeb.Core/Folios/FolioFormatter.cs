namespace PedidosWeb.Core.Folios;

/// <summary>
/// Genera el folio de un pedido con el mismo formato del sistema legado.
/// Las reglas completas y los ejemplos están en el README, sección «La función del taller».
/// </summary>
public static class FolioFormatter
{
    private const int NumeroMinimo = 1;
    private const int NumeroMaximo = 999999;
    private const int LargoMinimoSucursal = 2;
    private const int LargoMaximoSucursal = 4;

    /// <summary>
    /// Devuelve el folio con el formato PED-{SUCURSAL}-{NUMERO}, por ejemplo "PED-STGO-000123".
    /// </summary>
    /// <param name="numero">Número correlativo del pedido, entre 1 y 999999.</param>
    /// <param name="sucursal">Código de la sucursal: de 2 a 4 letras de la A a la Z.</param>
    /// <exception cref="ArgumentOutOfRangeException">Si <paramref name="numero"/> está fuera de 1 a 999999.</exception>
    /// <exception cref="ArgumentException">Si <paramref name="sucursal"/> es nula, vacía o no cumple el formato.</exception>
    public static string FormatearFolio(int numero, string sucursal)
    {
        // R1 y R5: el número se valida primero.
        if (numero < NumeroMinimo || numero > NumeroMaximo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numero), numero, $"El número debe estar entre {NumeroMinimo} y {NumeroMaximo}.");
        }

        // R2: sin espacios en los extremos y en mayúsculas.
        ArgumentNullException.ThrowIfNull(sucursal);
        var codigo = sucursal.Trim().ToUpperInvariant();

        // R3: de 2 a 4 letras de la A a la Z (ASCII).
        if (codigo.Length < LargoMinimoSucursal
            || codigo.Length > LargoMaximoSucursal
            || !codigo.All(letra => letra is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException(
                $"La sucursal debe tener de {LargoMinimoSucursal} a {LargoMaximoSucursal} letras de la A a la Z.",
                nameof(sucursal));
        }

        // R4: número con ceros a la izquierda hasta 6 dígitos.
        return $"PED-{codigo}-{numero:D6}";
    }
}
