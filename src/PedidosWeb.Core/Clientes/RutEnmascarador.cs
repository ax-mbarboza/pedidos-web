namespace PedidosWeb.Core.Clientes;

/// <summary>
/// Oculta el cuerpo de un RUT chileno para mostrarlo sin exponer el dato completo.
/// </summary>
public static class RutEnmascarador
{
    private const string RutOculto = "**.***.***-*";
    private const int DigitosVisibles = 3;

    /// <summary>
    /// Deja visibles los últimos 3 dígitos del cuerpo y el dígito verificador:
    /// <c>76.123.456-0</c> → <c>**.***.456-0</c>.
    /// Acepta el RUT como viene del legado (con puntos, con espacios, con la «k» en minúscula).
    /// Si no tiene el formato esperado, lo oculta por completo.
    /// </summary>
    public static string Enmascarar(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
        {
            return RutOculto;
        }

        var normalizado = rut.Replace(".", "").Replace(" ", "").ToUpperInvariant();
        var partes = normalizado.Split('-');

        if (partes.Length != 2
            || partes[1].Length != 1
            || partes[0].Length < DigitosVisibles
            || !partes[0].All(char.IsAsciiDigit))
        {
            return RutOculto;
        }

        var ultimosDigitos = partes[0][^DigitosVisibles..];
        return $"**.***.{ultimosDigitos}-{partes[1]}";
    }
}
