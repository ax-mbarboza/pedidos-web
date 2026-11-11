using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PedidosWeb.Api.Tests;

public class DetalleEndpointTests(WebApplicationFactory<Program> fabrica) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente = fabrica.CreateClient();

    // CA-01 a CA-04 por HTTP: el pedido 8 con su cliente enmascarado, sus líneas y su total.
    [Fact]
    public async Task GetDetalle_Pedido8_Responde200ConElDetalleCompleto()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/8/detalle");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var raiz = json.RootElement;

        var pedido = raiz.GetProperty("pedido");
        Assert.Equal("PED-STGO-000008", pedido.GetProperty("folio").GetString());
        Assert.Equal("2026-10-20", pedido.GetProperty("fecha").GetString());
        Assert.Equal("Entregado", pedido.GetProperty("estado").GetString());
        Assert.Equal("STGO", pedido.GetProperty("sucursal").GetString());

        var cliente = raiz.GetProperty("cliente");
        Assert.Equal("C-001", cliente.GetProperty("id").GetString());
        Assert.Equal("Ferretería El Clavo", cliente.GetProperty("nombre").GetString());
        Assert.Equal("**.***.456-0", cliente.GetProperty("rut").GetString());
        Assert.Equal("Zona centro", cliente.GetProperty("zona").GetString());
        Assert.True(cliente.GetProperty("activo").GetBoolean());

        var lineas = raiz.GetProperty("lineas");
        Assert.Equal(3, lineas.GetArrayLength());
        Assert.Equal("Harina para pan 25 kg", lineas[0].GetProperty("producto").GetString());
        Assert.Equal(64500m, lineas[0].GetProperty("subtotal").GetDecimal());

        Assert.Equal(120100m, raiz.GetProperty("total").GetDecimal());
    }

    // El RUT completo nunca debe salir en la respuesta.
    [Fact]
    public async Task GetDetalle_Pedido8_NoExponeElRutCompleto()
    {
        var cuerpo = await _cliente.GetStringAsync("/api/pedidos/8/detalle");

        Assert.DoesNotContain("76.123.456", cuerpo);
        Assert.DoesNotContain("76123456", cuerpo);
    }

    // CA-06
    [Fact]
    public async Task GetDetalle_PedidoInexistente_Responde404ConProblemDetails()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/999/detalle");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
    }

    // La ruta existente no se toca.
    [Fact]
    public async Task GetPedidoPorId_Pedido8_SigueRespondiendoElPedidoPlano()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/8");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("PED-STGO-000008", json.RootElement.GetProperty("folio").GetString());
        Assert.False(json.RootElement.TryGetProperty("lineas", out _));
    }
}
