using PedidosWeb.Core.Clientes;
using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

public class DetallePedidoServiceTests
{
    private static DetallePedidoService CrearServicio() => new(
        new PedidoRepositorioEnMemoria(),
        new LineaPedidoRepositorioEnMemoria(),
        new ClientesService());

    private static DetallePedido ObtenerDetalle(int id)
    {
        var resultado = CrearServicio().ObtenerDetalle(id);

        Assert.NotNull(resultado.Detalle);
        return resultado.Detalle;
    }

    // CA-01
    [Fact]
    public void ObtenerDetalle_Pedido8_DevuelveLosDatosDelPedido()
    {
        var detalle = ObtenerDetalle(8);

        Assert.Equal(new PedidoDetalle(8, "PED-STGO-000008", new DateOnly(2026, 10, 20), "Entregado", "STGO"), detalle.Pedido);
    }

    // CA-02
    [Fact]
    public void ObtenerDetalle_Pedido8_DevuelveElClienteConElRutEnmascarado()
    {
        var detalle = ObtenerDetalle(8);

        Assert.Equal(
            new ClienteDetalle("C-001", "Ferretería El Clavo", "**.***.456-0", "Zona centro", true),
            detalle.Cliente);
    }

    // CA-03
    [Fact]
    public void ObtenerDetalle_Pedido8_DevuelveSusTresLineasEnOrdenConSubtotal()
    {
        var detalle = ObtenerDetalle(8);

        Assert.Equal(
            new[]
            {
                new LineaDetalle(1, "Harina para pan 25 kg", 3, 21500m, 64500m),
                new LineaDetalle(2, "Mantequilla 250 g", 12, 2950m, 35400m),
                new LineaDetalle(3, "Bebida cola 1,5 L", 10, 2020m, 20200m),
            },
            detalle.Lineas);
    }

    // CA-04
    [Fact]
    public void ObtenerDetalle_Pedido8_TotalIgualASumaDeSubtotales()
    {
        var detalle = ObtenerDetalle(8);

        Assert.Equal(120100m, detalle.Total);
        Assert.Equal(detalle.Total, detalle.Lineas.Sum(linea => linea.Subtotal));
    }

    // CA-05
    [Fact]
    public void ObtenerDetalle_Los45PedidosDeEjemplo_TotalIgualASumaDeSubtotales()
    {
        var pedidos = new PedidoRepositorioEnMemoria().ObtenerTodos();

        Assert.Equal(45, pedidos.Count);
        foreach (var pedido in pedidos)
        {
            var detalle = ObtenerDetalle(pedido.Id);

            Assert.Equal(pedido.Total, detalle.Lineas.Sum(linea => linea.Subtotal));
            Assert.Equal(pedido.Total, detalle.Total);
        }
    }

    // CA-06 (a nivel de Core: la Api lo traduce a 404)
    [Fact]
    public void ObtenerDetalle_PedidoInexistente_DevuelveMotivoPedidoNoExiste()
    {
        var resultado = CrearServicio().ObtenerDetalle(999);

        Assert.Null(resultado.Detalle);
        Assert.Equal(MotivoSinDetalle.PedidoNoExiste, resultado.Motivo);
    }

    // R3: el cliente sin ficha responde 404.
    [Fact]
    public void ObtenerDetalle_ClienteSinFicha_DevuelveMotivoClienteSinFicha()
    {
        var pedidos = new PedidoRepositorioDePrueba(
            new Pedido(1, "PED-STGO-000001", "C-999", "Cliente Fantasma", new DateOnly(2026, 10, 1), "Pendiente", 1000m, "STGO"));
        var servicio = new DetallePedidoService(pedidos, new LineaPedidoRepositorioEnMemoria(), new ClientesService());

        var resultado = servicio.ObtenerDetalle(1);

        Assert.Null(resultado.Detalle);
        Assert.Equal(MotivoSinDetalle.ClienteSinFicha, resultado.Motivo);
    }

    // R3: un cliente inactivo igual se muestra, con activo = false.
    [Fact]
    public void ObtenerDetalle_ClienteInactivo_LoMuestraConActivoFalso()
    {
        var detalle = ObtenerDetalle(6); // C-007, Kiosco Central

        Assert.Equal("C-007", detalle.Cliente.Id);
        Assert.False(detalle.Cliente.Activo);
    }

    // Decisión 4: un pedido anulado se muestra igual, con su estado.
    [Fact]
    public void ObtenerDetalle_PedidoAnulado_LoMuestraConSuEstado()
    {
        var detalle = ObtenerDetalle(29);

        Assert.Equal("Anulado", detalle.Pedido.Estado);
    }

    // Un nombre con " | " no debe desarmar los demás campos de la ficha.
    [Fact]
    public void ObtenerDetalle_NombreConSeparador_ConservaElNombreCompleto()
    {
        var clientes = new ClientesService(new List<Cliente>
        {
            new() { Id = "C-100", Nombre = "Mercado | Central", Rut = "76.123.456-0", Region = "RM", Activo = true },
        });
        var pedidos = new PedidoRepositorioDePrueba(
            new Pedido(1, "PED-STGO-000001", "C-100", "Mercado | Central", new DateOnly(2026, 10, 1), "Pendiente", 1000m, "STGO"));
        var servicio = new DetallePedidoService(pedidos, new LineaPedidoRepositorioEnMemoria(), clientes);

        var detalle = servicio.ObtenerDetalle(1).Detalle;

        Assert.NotNull(detalle);
        Assert.Equal(new ClienteDetalle("C-100", "Mercado | Central", "**.***.456-0", "Zona centro", true), detalle.Cliente);
    }

    private sealed class PedidoRepositorioDePrueba(params Pedido[] pedidos) : IPedidoRepositorio
    {
        public IReadOnlyList<Pedido> ObtenerTodos() => pedidos;

        public Pedido? ObtenerPorId(int id) => pedidos.FirstOrDefault(p => p.Id == id);
    }
}
