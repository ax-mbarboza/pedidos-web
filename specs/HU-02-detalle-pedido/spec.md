# HU-02 · Detalle de un pedido

> Estado: **APROBADA.** El plan de implementación está en `plan.md`.

**Historia:** Como vendedor, quiero ver el detalle de un pedido, para confirmar con el cliente qué
pidió, a qué precio y en qué estado está.

## Qué devuelve

La ruta `GET /api/pedidos/{id}/detalle` devuelve (la ruta `GET /api/pedidos/{id}` no se toca):

- Del pedido: `id`, `folio`, `fecha` (`yyyy-MM-dd`), `estado`, `sucursal`.
- Del cliente: `id`, `nombre`, `rut` (enmascarado), `zona`, `activo`.
- De cada línea (ordenadas por `numero`): `numero`, `producto`, `cantidad`, `precioUnitario` y `subtotal`
  (`cantidad × precioUnitario`).
- Total: `total` del pedido, en CLP, entero y sin IVA.

## Reglas

- R1. Los montos son `decimal` en CLP, sin decimales. Los precios son sin IVA y no se calcula IVA.
- R2. `total` es el `Pedido.Total` y coincide con la suma de los `subtotal` de sus líneas.
- R3. Los datos del cliente salen de `ClientesService` (`ObtenerFichaTexto`) por el `ClienteId` del pedido. Un cliente
  inactivo igual se muestra, con `activo = false`. Si el cliente no tiene ficha, responde 404.
- R3b. El RUT se muestra enmascarado, ocultando el cuerpo: quedan los últimos 3 dígitos del cuerpo y el dígito
  verificador (`76.123.456-0` → `**.***.456-0`). La zona viene de `ObtenerFichaTexto` («Zona centro», «Zona sur»,
  «Zona norte» o «Sin zona»); el detalle no trae la región.
- R4. Los errores usan ProblemDetails (`application/problem+json`), igual que la HU-01.
- R5. La lógica de armado vive en `PedidosWeb.Core`; la Api solo traduce HTTP.

## Criterios de aceptación

- **CA-01** · Dado el pedido 8, cuando pido su detalle, entonces responde 200 con folio `PED-STGO-000008`,
  fecha `2026-10-20`, estado `Entregado` y sucursal `STGO`.
- **CA-02** · Dado el pedido 8, cuando pido su detalle, entonces el cliente es `C-001`, «Ferretería El Clavo»,
  RUT `**.***.456-0`, zona «Zona centro», activo.
- **CA-03** · Dado el pedido 8, cuando pido su detalle, entonces trae 3 líneas en orden: «Harina para pan 25 kg»
  (3 × 21.500 = 64.500), «Mantequilla 250 g» (12 × 2.950 = 35.400) y «Bebida cola 1,5 L» (10 × 2.020 = 20.200).
- **CA-04** · Dado el pedido 8, cuando pido su detalle, entonces `total = 120100`, igual a la suma de los subtotales.
- **CA-05** · Dado cualquiera de los 45 pedidos de ejemplo, cuando pido su detalle, entonces la suma de los
  subtotales es igual a `total`.
- **CA-06** · Dado el id 999, cuando pido su detalle, entonces responde 404 ProblemDetails con `status = 404`.

## Si el pedido no existe

Responde 404 con ProblemDetails (`status = 404`). Un id no numérico (`/api/pedidos/abc/detalle`) no coincide
con la ruta y cae en el 404 por defecto de ASP.NET.

## Ejemplo

`GET /api/pedidos/8/detalle` → 200:
`{ "pedido": { "id": 8, "folio": "PED-STGO-000008", "fecha": "2026-10-20", "estado": "Entregado", "sucursal": "STGO" }, "cliente": { "id": "C-001", "nombre": "Ferretería El Clavo", "rut": "**.***.456-0", "zona": "Zona centro", "activo": true }, "lineas": [{ "numero": 1, "producto": "Harina para pan 25 kg", "cantidad": 3, "precioUnitario": 21500, "subtotal": 64500 }, …], "total": 120100 }`

## Fuera de alcance

- Crear, editar o anular pedidos (solo lectura).
- Calcular IVA o descuentos: el cálculo de totales migrado del legado llega en una unidad posterior.
- Autenticación y permisos.
- Cambiar `GET /api/pedidos/{id}` o `ClientesService` (el módulo heredado se refactoriza aparte. Solo se
  renombró `proc2` a `ObtenerFichaTexto`, sin cambiar su comportamiento; `proc3` se mantiene tal cual).

## Decisiones tomadas

1. **Campos del cliente.** `id`, `nombre`, `rut` enmascarado, `zona` y `activo`.
2. **Cliente sin ficha.** Si el `ClienteId` del pedido no existe en `ClientesService`, responde 404.
3. **Uso de `ClientesService`.** Se usa `ObtenerFichaTexto` (antes `proc2`, renombrado sin
   cambiar su comportamiento); `proc3` se mantiene sin cambios. `ObtenerFichaTexto` devuelve la zona y no la región, por eso el campo es `zona`.
4. **Pedido anulado.** Se muestra igual, con su estado.
5. **Línea con subtotal.** `subtotal` está presente en cada línea.

## Preguntas abiertas

- Ninguna por ahora.
