# HU-01 · Listar pedidos

## Historia de usuario
Como vendedor, quiero listar pedidos filtrando por cliente, estado y rango de fechas.

## Objetivo
Exponer `GET /api/pedidos` para consultar los pedidos de ejemplo aplicando de forma opcional los filtros
cliente, estado y rango de fechas. El resultado se ordena del más reciente al más antiguo y se pagina.
El filtrado vive en `PedidosWeb.Core`; la Api solo traduce parámetros HTTP y códigos de respuesta.

## Reglas de negocio
- R1. Los tres filtros son opcionales y se combinan con AND. Sin filtros se devuelve todo (paginado).
- R2. Cliente: se filtra por `ClienteId` exacto (ej. `C-001`). Un cliente "existe" si aparece en al menos un pedido.
- R3. Estado: se filtra por el texto del estado. Los estados válidos son 'Pendiente', 'Confirmado', 'Despachado', 'Entregado', 'Anulado'. Debe distinguirse mayúsculas tal cuál están dados.
- R4. Fechas: `desde` y `hasta` son inclusivos (formato `yyyy-MM-dd`, `DateOnly`). Cada extremo es opcional e independiente: se puede enviar solo uno, o ambos.
- R5. Orden: `Fecha` descendente; a igual fecha, `Id` ascendente.
- R6. El tamaño de página por defecto es 20 y el máximo es 40.
- R7. En la inexistencia de un cliente, devolver un 404.
- R8. Parámetros de query: `cliente`, `estado`, `desde`, `hasta`, `pagina` (base 1, por defecto 1) y `tamanoPagina` (por defecto 20).
- R9. `pagina` < 1, `tamanoPagina` < 1 o > 40, o valores no numéricos → 400. Se rechaza, no se recorta en silencio.
- R10. La respuesta es un objeto con `items` (la lista de la página), `total`, `pagina` y `tamanoPagina`. `total` es la cantidad de pedidos que cumplen los filtros, antes de paginar. Una página fuera de rango devuelve 200, `items` vacío y el `total` correcto.
- R11. Un filtro presente pero vacío (`estado=`, `cliente=`) → 400; no se ignora.
- R12. El filtro de cliente distingue mayúsculas, igual que el de estado (`c-001` → 404).
- R13. Precedencia de errores: primero se validan formato, estado, fechas y paginación (400); después se comprueba que el cliente exista (404).
- R14. Sin filtro de estado se incluyen los pedidos `Anulado`.
- R15. Cada ítem trae los 8 campos del `Pedido`; `fecha` se serializa como `yyyy-MM-dd`.
- R16. Los errores usan ProblemDetails (`application/problem+json`) y su `status` coincide con el código HTTP.
- R17. Los parámetros de query desconocidos se ignoran.

## Criterios de aceptación
- [ ] CA1. Sin filtros devuelve la página 1 con 20 pedidos y `total = 45`, ordenados por `Fecha` descendente.
- [ ] CA2. `cliente=C-001` devuelve 5 pedidos, ids 8, 16, 24, 32, 40, en ese orden.
- [ ] CA3. `cliente=C-001&estado=Entregado` devuelve 3 pedidos: ids 8, 24, 40.
- [ ] CA4. `desde=2026-10-30&hasta=2026-10-31` devuelve ids 21 (31-oct) y 42 (30-oct): ambos extremos incluidos.
- [ ] CA5. `desde=2026-09-02&hasta=2026-09-02` devuelve solo el id 40.
- [ ] CA6. `estado=Pendiente` devuelve 5 pedidos: ids 27, 18, 45, 9, 36.
- [ ] CA7. `cliente=C-001&estado=Pendiente` devuelve 200, `items` vacío y `total = 0` (C-001 no tiene pedidos pendientes).
- [ ] CA8. `desde=2026-10-31&hasta=2026-10-01` (desde posterior a hasta) devuelve 400 ProblemDetails con `status = 400`.
- [ ] CA9. Sin filtros, página 1 (20 ítems): `total = 45` y los primeros ids son 21, 42, 2, 23, 44.
- [ ] CA10. `pagina=3`: 5 ítems, ids 36, 17, 38, 19, 40; `total = 45`.
- [ ] CA11. `pagina=4`: 200, `items` vacío, `total = 45`.
- [ ] CA12. `tamanoPagina=2`: ids 21 y 42; `total = 45`.
- [ ] CA13. `tamanoPagina=41`, `tamanoPagina=0` y `pagina=0` → 400 ProblemDetails.
- [ ] CA14. Solo `desde=2026-10-29`: ids 21, 42, 2 (`total = 3`).
- [ ] CA15. Solo `hasta=2026-09-03`: ids 19 y 40 (`total = 2`).
- [ ] CA16. Los tres filtros: `cliente=C-003&estado=Entregado&desde=2026-10-01&hasta=2026-10-31` devuelve solo el id 42.
- [ ] CA17. `estado=Anulado&desde=2026-10-01&hasta=2026-10-31`: ids 2, 29, 20, en ese orden.
- [ ] CA18. Rango sin datos (`desde=2026-11-01&hasta=2026-11-30`): 200, `items` vacío, `total = 0`.
- [ ] CA19. `cliente=C-999` → 404 ProblemDetails.
- [ ] CA20. `cliente=c-001` (minúscula) → 404.
- [ ] CA21. `estado=pendiente` (minúscula) y `estado=Enviado` → 400.
- [ ] CA22. `desde=2026-13-01` y `desde=hoy` → 400.
- [ ] CA23. `cliente=C-999&estado=Enviado` → 400 (la validación va antes que el 404, R13).
- [ ] CA24. Empate de fecha: dos pedidos del mismo día salen ordenados por `Id` ascendente. Las 45 fechas de ejemplo son distintas, así que este test usa un repositorio de prueba con datos propios.

## Ejemplos de entrada y salida
1. `GET /api/pedidos?cliente=C-002&estado=Anulado` → 200:
   `{ "items": [{ "id": 29, "folio": "PED-ANT-000029", "clienteId": "C-002", "clienteNombre": "Minimarket Doña Rosa", "fecha": "2026-10-19", "estado": "Anulado", "total": 66050, "sucursal": "ANT" }], "total": 1, "pagina": 1, "tamanoPagina": 20 }`
2. `GET /api/pedidos?cliente=C-001&estado=Pendiente` → 200, `{ "items": [], "total": 0, "pagina": 1, "tamanoPagina": 20 }`.
3. Error: `GET /api/pedidos?desde=2026-10-31&hasta=2026-10-01` → 400, `application/problem+json` con `status = 400`.
4. Error: `GET /api/pedidos?cliente=C-999` → 404, `application/problem+json` con `status = 404`.

## Errores esperados
- 400 si `desde` es posterior a `hasta`.
- 400 si una fecha o un número de página tiene formato inválido, si el estado no es uno de los 5 válidos o si un filtro viene vacío.
- 400 si `tamanoPagina` está fuera de 1 a 40 o `pagina` es menor que 1.
- 404 si el cliente no existe en los pedidos.
- Todos los errores usan ProblemDetails.

## Fuera de alcance
- Crear, editar o anular pedidos (HU-01 es solo lectura).
- Autenticación y permisos (no se valida quién es el vendedor ni qué clientes puede ver).
- Búsqueda por texto libre (nombre de cliente, folio).
- Filtros extra (sucursal, monto) y exportación (CSV/Excel).

## Preguntas abiertas
- Ninguna por ahora.
