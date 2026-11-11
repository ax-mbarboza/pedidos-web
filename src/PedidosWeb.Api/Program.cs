using PedidosWeb.Core.Clientes;
using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPedidoRepositorio, PedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<ILineaPedidoRepositorio, LineaPedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<ClientesService>();
builder.Services.AddSingleton<DetallePedidoService>();

var app = builder.Build();

// Para comprobar que la API está arriba.
app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

// Detalle de un pedido por id.
app.MapGet("/api/pedidos/{id:int}", (int id, IPedidoRepositorio repositorio) =>
    repositorio.ObtenerPorId(id) is { } pedido
        ? Results.Ok(pedido)
        : Results.NotFound());

// HU-02 · Detalle de un pedido: cliente, líneas, total y estado.
// Si el pedido no existe, o su cliente no tiene ficha, responde 404 con ProblemDetails.
app.MapGet("/api/pedidos/{id:int}/detalle", (int id, DetallePedidoService servicio) =>
{
    var resultado = servicio.ObtenerDetalle(id);

    return resultado.Detalle is { } detalle
        ? Results.Ok(detalle)
        : Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "No se encontró el detalle del pedido.",
            detail: resultado.Motivo == MotivoSinDetalle.ClienteSinFicha
                ? $"El pedido {id} existe, pero su cliente no tiene ficha."
                : $"No existe un pedido con id {id}.");
});

// HU-01 · Listar pedidos con filtros: PENDIENTE.
// Se construye en el taller de la Unidad 2, a partir de specs/HU-01-listar-pedidos/spec.md.

app.Run();

// Permite crear tests de integración con WebApplicationFactory<Program> más adelante.
public partial class Program { }
