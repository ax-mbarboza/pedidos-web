# Riesgos de Pedidos Legado

Las rutas son relativas a `legado/src/`. Cada riesgo tiene evidencia que leí en el código. Prioridad: Alta, Media o Baja según gravedad y probabilidad. La columna Riesgo separa el hecho de la interpretación cuando importa.

| ID | Riesgo | Evidencia (archivo:línea) | Categoría | Prioridad |
|---|---|---|---|---|
| RS-01 | **Clave de la base de datos escrita en el código.** Hecho: `CLAVE_BD = 'Distrib#2008'` y se registra con `Session.AddPassword`. Quien tenga el fuente tiene la clave. | `UDMPrincipal.pas:41`, `:59` | Seguridad | Alta |
| RS-02 | **SQL armado por concatenación, sin escapar.** Hecho: `gUsuario` y las observaciones del pedido se insertan directo en el `INSERT`; la búsqueda de clientes concatena `edtBuscar.Text`. Existe `EscapaComillas` en `UUtilidades.pas:164`, pero estas consultas no la usan. Interpretación: una comilla en una observación rompe el pedido, y la búsqueda permite inyección SQL. | `UFrmPedidos.pas:259-262`; `UFrmClientes.pas:76-77` | Seguridad | Alta |
| RS-03 | **Pedido sin transacción.** Hecho: el código tiene `// TODO transaccion` y graba cabecera, líneas, total y stock en sentencias separadas. Interpretación: un error a mitad deja un pedido incompleto, sin total o sin descuento de stock. | `UFrmPedidos.pas:255`, `:259-273`, `:320`, `:324` | Datos e integridad | Alta |
| RS-04 | **Descuento calculado en dos lugares con reglas distintas.** Hecho: `UCalculoTotales` usa `>` y `UFrmPedidos` usa `>=`; Navidad solo existe en `UFrmPedidos`. Con base exactamente 500.000 o 1.000.000, el total guardado y el mostrado difieren. El aviso de diferencia solo sale si `gModoPrueba` es verdadero, y nace en `False`. | `UCalculoTotales.pas:77-80`; `UFrmPedidos.pas:296-307`, `:327-332`; `UGlobales.pas:34` | Datos e integridad | Alta |
| RS-05 | **El pedido se graba aunque falte stock.** Hecho: `DescontarStockPedido` se llama después de grabar y, si no alcanza, solo muestra un mensaje. El pedido y su total ya quedaron guardados. | `UFrmPedidos.pas:324`; `UStock.pas:69-72` | Datos e integridad | Alta |
| RS-06 | **Folio e ID de cliente con `MAX + 1`.** Hecho: se lee el máximo y se suma 1 sin bloqueo. Interpretación: con dos usuarios a la vez pueden repetirse. | `UFrmPedidos.pas:249-251`; `UFrmClientes.pas:160-163` | Datos e integridad | Media |
| RS-07 | **Dos validaciones de RUT con el dígito verificador invertido.** Hecho: `RutOk` devuelve `K` con resto 11 y `0` con resto 10; `ValidaRut` hace lo contrario. Interpretación: un RUT válido puede rechazarse al crear el cliente, o un inválido aceptarse. | `UUtilidades.pas:72-79`; `UFrmClientes.pas:192-199` | Datos e integridad | Media |
| RS-08 | **`MOV_STOCK` sin definición y con errores tragados.** Hecho: la tabla no está en el esquema ni en los CSV, y el `except` vacío ignora cualquier error del `INSERT`. Interpretación: si la tabla no existe en producción, los movimientos de stock no se registran y nadie se entera. | `UStock.pas:85-90` | Datos e integridad | Media |
| RS-09 | **IVA fijo en el informe.** Hecho: el informe divide por `1 + 0.19` en vez de usar `gTasaIVA`. Si la tasa cambia, el informe queda desfasado. | `UFrmInformes.pas:92` | Datos e integridad | Media |
| RS-10 | **Parámetros que no se respetan.** Hecho: `PEDIDO_MINIMO` (25.000) y `DESC_NAVIDAD` están en PARAMETROS, pero el código usa 30.000 fijo y `gDescNavidad` queda en `False`. Cambiar la tabla no cambia el comportamiento. | `UFrmPedidos.pas:231`, `:304`; `UGlobales.pas:33` | Mantenibilidad | Media |
| RS-11 | **Cierre de pedidos sin feriados y con la hora del equipo.** Hecho: `FechaParaPedido` salta solo sábado y domingo y usa `Now` del PC cliente. Interpretación: un pedido ingresado en vísperas de feriado queda con fecha de feriado, y un reloj mal configurado cambia la fecha. | `UUtilidades.pas:122-140`; `UFrmPedidos.pas:240` | Fechas | Media |
| RS-12 | **Fechas con año de 2 dígitos.** Hecho: `StrAFecha` interpreta años menores que 50 como 20xx y el resto como 19xx. Interpretación: fallará en 2050. | `UUtilidades.pas:96-108` | Fechas | Baja |
| RS-13 | **El nivel «supervisor» no se puede asignar.** Hecho: el nivel parte en 1 y solo sube a 9. Crear clientes VIP exige nivel 2 o más, así que en la práctica solo un administrador puede. | `UGlobales.pas:30`; `UFrmPrincipal.pas:50`; `UFrmClientes.pas:143` | Seguridad | Baja |

## Preguntas abiertas

1. ¿Se usa `MOV_STOCK` en producción? ¿Dónde está su definición?
2. ¿Hay otros usuarios que operen a la vez sobre la misma base? Eso decide la prioridad de RS-06.
3. ¿Quién registra los movimientos de stock hoy si `MOV_STOCK` no existe?
4. ¿La clave de la base ya está en uso en algún entorno real? Si es así, habría que rotarla al migrar.
