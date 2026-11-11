# Plan · HU-02 Detalle de un pedido

> Estado: **APROBADO** (plan y las dos aprobaciones explícitas).

## Contexto

La spec `specs/HU-02-detalle-pedido/spec.md` define `GET /api/pedidos/{id}/detalle`. Decisiones ya tomadas:

- El RUT sale enmascarado, ocultando el cuerpo.
- La ficha del cliente se obtiene con `ObtenerFichaTexto` (antes `proc2`; ver «Cambios posteriores»).
- La Api tiene un proyecto de tests homónimo.

## Plan en 5 viñetas

1. **Spec:** pasar el estado a «Aprobada» y alinear dos puntos que hoy chocan con las decisiones.
   - CA-02, el ejemplo y R3: el RUT sale enmascarado (`**.***.456-0`).
   - `ObtenerFichaTexto` entrega la **zona** («Zona centro»), no la región. El campo `region` pasa a `zona` en
     la spec, el ejemplo y CA-02. Es la consecuencia de usar ese método.
   - Cerrar «Preguntas abiertas» y la línea de Fuera de alcance sobre `ClientesService` (no se modifica).
2. **Core** (`src/PedidosWeb.Core/`), sin tocar `ClientesService`, `Pedido` ni `LineaPedido`:
   - `Pedidos/DetallePedido.cs`: records del resultado (pedido, cliente, líneas con `Subtotal`, `Total`).
   - `Pedidos/DetallePedidoService.cs`: recibe `IPedidoRepositorio`, `ILineaPedidoRepositorio` y `ClientesService`.
     `ObtenerDetalle(id)` arma el detalle y distingue «pedido no existe» y «cliente sin ficha». Ambos casos
     terminan en 404 en la Api.
   - Ficha del cliente: llama a `ObtenerFichaTexto(clienteId)`. Si devuelve `""`, el cliente no existe. Si no, parte el
     texto por `" | "` en nombre, RUT, zona y estado (`"Activo"` → `activo = true`).
   - `Clientes/RutEnmascarador.cs`: `76123456-0` → `**.***.456-0`, con los últimos 3 dígitos del cuerpo y el
     dígito verificador (el RUT ya llega normalizado desde `ObtenerFichaTexto`).
   - `Subtotal = Cantidad × PrecioUnitario`, en `decimal`. Un pedido anulado se muestra igual.
3. **Api** (`src/PedidosWeb.Api/Program.cs`): registrar `ILineaPedidoRepositorio`, `ClientesService` y
   `DetallePedidoService`, y agregar `MapGet("/api/pedidos/{id:int}/detalle")`. Pedido inexistente o cliente
   sin ficha → `Results.Problem(statusCode: 404)` (ProblemDetails). La ruta `GET /api/pedidos/{id}` no se toca.
4. **Tests** (un archivo por clase):
   - `tests/PedidosWeb.Core.Tests/Pedidos/DetallePedidoServiceTests.cs`: CA-01 a CA-05 (el CA-05 recorre los 45
     pedidos), pedido inexistente, cliente sin ficha (con un `ClientesService` de lista propia), cliente
     inactivo (C-007) y pedido anulado.
   - `tests/PedidosWeb.Core.Tests/Clientes/RutEnmascaradorTests.cs`: RUT con puntos, con espacios, con `k`
     minúscula y sin puntos.
   - Proyecto nuevo `tests/PedidosWeb.Api.Tests/` con `DetalleEndpointTests.cs` sobre
     `WebApplicationFactory<Program>`. CA-06: 404 con `application/problem+json` y `status = 404`. Además, un
     200 para el pedido 8 con el RUT enmascarado. `Program` ya es `public partial class` para esto.
5. **Validar:** `dotnet build` (0 warnings) y `dotnet test` en verde, más una llamada real con
   `dotnet run --project src/PedidosWeb.Api` a `/api/pedidos/8/detalle` y `/api/pedidos/999/detalle`.
   Sin commit ni push.

## Aprobaciones explícitas

Estas dos salen de reglas de `AGENTS.md` y necesitan un OK aparte del plan general. Marca cada una.

- [x] **Paquete NuGet nuevo:** `Microsoft.AspNetCore.Mvc.Testing` (última 10.x estable), solo en
  `PedidosWeb.Api.Tests`. Sin esto, CA-06 no tiene test automático.
- [x] **`PedidosWeb.sln`:** agregar `PedidosWeb.Api.Tests` con `dotnet sln add`. El archivo está en la raíz,
  fuera de las carpetas permitidas (`src/`, `tests/`, `docs/`, `specs/`).

## Riesgos y límites

- Leer la ficha parseando el texto de `ObtenerFichaTexto` es frágil: si un nombre de cliente incluyera `" | "`, se parte
  mal. Con los 8 clientes de ejemplo no pasa. Queda como deuda para cuando se refactorice `ClientesService`.
- `proc3` no se usa: `ObtenerFichaTexto` ya informa el estado y distingue «no existe» de «inactivo».

## Cambios posteriores

- **Renombre de `proc2` → `ObtenerFichaTexto`** (`ClientesService`). Solo cambió el nombre: la firma, el cuerpo
  y el resultado son los mismos, y ahora el método tiene comentario XML con el formato del texto
  (`"nombre | rut | zona | estado"`, o `""` si el cliente no existe). Se actualizaron sus 3 llamadas en
  `ClientesServiceTests` (solo el nombre, sin tocar aserciones) y la de `DetallePedidoService`.
  Los 3 tests de la ficha pasaron de `Proc2_...` a `ObtenerFichaTexto_...` para que sus nombres coincidan con
  el método (`Metodo_Escenario_ResultadoEsperado`). `proc3` no se renombró por ahora.

## Archivos

- Modificar: `specs/HU-02-detalle-pedido/spec.md`, `src/PedidosWeb.Api/Program.cs`, `PedidosWeb.sln`.
- Nuevos: `src/PedidosWeb.Core/Pedidos/DetallePedido.cs`, `.../DetallePedidoService.cs`,
  `src/PedidosWeb.Core/Clientes/RutEnmascarador.cs`, los dos archivos de test de Core y el proyecto
  `tests/PedidosWeb.Api.Tests/` (`.csproj` y `DetalleEndpointTests.cs`).
