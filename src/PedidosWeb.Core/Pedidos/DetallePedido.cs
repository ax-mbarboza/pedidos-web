namespace PedidosWeb.Core.Pedidos;

/// <summary>El detalle de un pedido: sus datos, su cliente, sus líneas y su total.</summary>
/// <param name="Pedido">Datos del pedido.</param>
/// <param name="Cliente">Ficha del cliente, con el RUT enmascarado.</param>
/// <param name="Lineas">Líneas del pedido, ordenadas por número.</param>
/// <param name="Total">Total del pedido en pesos chilenos (CLP), sin IVA: <c>decimal</c> sin decimales.</param>
public sealed record DetallePedido(
    PedidoDetalle Pedido,
    ClienteDetalle Cliente,
    IReadOnlyList<LineaDetalle> Lineas,
    decimal Total);

/// <summary>Datos del pedido que se muestran en el detalle.</summary>
public sealed record PedidoDetalle(int Id, string Folio, DateOnly Fecha, string Estado, string Sucursal);

/// <summary>Datos del cliente que se muestran en el detalle.</summary>
/// <param name="Id">Código del cliente, por ejemplo "C-001".</param>
/// <param name="Nombre">Nombre del cliente.</param>
/// <param name="Rut">RUT enmascarado, por ejemplo "**.***.456-0".</param>
/// <param name="Zona">Zona del cliente, por ejemplo "Zona centro".</param>
/// <param name="Activo">Si el cliente está activo.</param>
public sealed record ClienteDetalle(string Id, string Nombre, string Rut, string Zona, bool Activo);

/// <summary>Una línea del detalle, con su subtotal.</summary>
/// <param name="Subtotal"><c>Cantidad × PrecioUnitario</c> en CLP, sin IVA.</param>
public sealed record LineaDetalle(int Numero, string Producto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

/// <summary>Por qué no se pudo armar el detalle.</summary>
public enum MotivoSinDetalle
{
    /// <summary>No hay un pedido con ese id.</summary>
    PedidoNoExiste,

    /// <summary>El pedido existe, pero su cliente no tiene ficha.</summary>
    ClienteSinFicha,
}

/// <summary>Resultado de pedir el detalle: el detalle, o el motivo por el que no existe.</summary>
public sealed record ResultadoDetalle
{
    public DetallePedido? Detalle { get; private init; }

    public MotivoSinDetalle? Motivo { get; private init; }

    public static ResultadoDetalle Encontrado(DetallePedido detalle) => new() { Detalle = detalle };

    public static ResultadoDetalle SinDetalle(MotivoSinDetalle motivo) => new() { Motivo = motivo };
}
