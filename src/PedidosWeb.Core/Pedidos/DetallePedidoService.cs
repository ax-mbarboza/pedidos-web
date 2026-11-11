using PedidosWeb.Core.Clientes;
using PedidosWeb.Core.Lineas;

namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Arma el detalle de un pedido (HU-02) juntando el pedido, su cliente y sus líneas.
/// </summary>
public class DetallePedidoService
{
    private const string Separador = " | ";

    private readonly IPedidoRepositorio _pedidos;
    private readonly ILineaPedidoRepositorio _lineas;
    private readonly ClientesService _clientes;

    public DetallePedidoService(
        IPedidoRepositorio pedidos,
        ILineaPedidoRepositorio lineas,
        ClientesService clientes)
    {
        _pedidos = pedidos;
        _lineas = lineas;
        _clientes = clientes;
    }

    /// <summary>
    /// El detalle del pedido con ese id. Si el pedido no existe, o su cliente no tiene ficha,
    /// devuelve el motivo en lugar del detalle.
    /// </summary>
    public ResultadoDetalle ObtenerDetalle(int id)
    {
        var pedido = _pedidos.ObtenerPorId(id);
        if (pedido is null)
        {
            return ResultadoDetalle.SinDetalle(MotivoSinDetalle.PedidoNoExiste);
        }

        var cliente = ObtenerCliente(pedido.ClienteId);
        if (cliente is null)
        {
            return ResultadoDetalle.SinDetalle(MotivoSinDetalle.ClienteSinFicha);
        }

        var lineas = _lineas.ObtenerPorPedido(pedido.Id)
            .OrderBy(linea => linea.Numero)
            .Select(linea => new LineaDetalle(
                linea.Numero,
                linea.Producto,
                linea.Cantidad,
                linea.PrecioUnitario,
                linea.Cantidad * linea.PrecioUnitario))
            .ToList();

        var detalle = new DetallePedido(
            new PedidoDetalle(pedido.Id, pedido.Folio, pedido.Fecha, pedido.Estado, pedido.Sucursal),
            cliente,
            lineas,
            pedido.Total);

        return ResultadoDetalle.Encontrado(detalle);
    }

    // La ficha viene de ClientesService.ObtenerFichaTexto como texto: "nombre | rut | zona | estado".
    // Se lee desde el final para que un nombre con " | " no desarme los demás campos.
    private ClienteDetalle? ObtenerCliente(string clienteId)
    {
        var ficha = _clientes.ObtenerFichaTexto(clienteId);
        if (string.IsNullOrEmpty(ficha))
        {
            return null;
        }

        var partes = ficha.Split(Separador);
        if (partes.Length < 4)
        {
            return null;
        }

        var nombre = string.Join(Separador, partes[..^3]);
        var rut = partes[^3];
        var zona = partes[^2];
        var activo = partes[^1] == "Activo";

        return new ClienteDetalle(clienteId, nombre, RutEnmascarador.Enmascarar(rut), zona, activo);
    }
}
