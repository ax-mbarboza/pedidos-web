# migracion/

`golden-master.csv` es el **contrato** de la migración del cálculo de totales: 60 casos, uno por pedido de `legado/db/datos/PEDIDOS.csv`, con lo que calcula hoy `legado/src/UCalculoTotales.pas`, **defectos incluidos**.

| Columna | Qué es |
|---|---|
| `caso` | Número correlativo (1 a 60). |
| `folio` | Folio del pedido en el legado. |
| `tipo_cliente` | `N` normal o `V` VIP (`CLIENTES.csv`). |
| `region` | Código de región del cliente. |
| `flete_base` | Flete base de la región (`REGIONES.csv`). |
| `tasa_iva` | Tasa de IVA como fracción (`0.19`). |
| `lineas` | Líneas del pedido: `cantidad x precio x categoría`, separadas por `\|`. Ejemplo: `5x8210xBEB\|30x5000xCON`. |
| `subtotal` · `base_descuento` · `porc_descuento` · `descuento` · `flete` · `neto` · `iva` · `total` | Lo que calcula hoy el legado. |

- Los números usan **punto decimal**: léelos con `CultureInfo.InvariantCulture`.
- **No se edita para que un test pase.** Solo cambia en la etapa 3, cuando corriges un defecto, y cada caso que cambia queda registrado en `docs/migracion/defectos-corregidos.md`.
- El legado no compila, así que estos valores salen de una réplica de `UCalculoTotales.pas` hecha por el instructor. En un proyecto real se capturan ejecutando el sistema viejo.
