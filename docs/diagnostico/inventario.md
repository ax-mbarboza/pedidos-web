# Inventario de Pedidos Legado

Fuente: exploración de `legado/` con el subagente `analista-legado` (solo lectura). Las líneas se contaron con `wc -l`. Las responsabilidades se infieren de la lectura inicial del agente; las reglas de negocio se verifican en `reglas-negocio.md`.

## Resumen por extensión

25 archivos, 2.812 líneas.

| Extensión | Archivos | Líneas |
|---|---|---|
| `.pas` | 10 | 1.551 |
| `.dfm` | 6 | 824 |
| `.csv` | 6 | 327 |
| `.sql` | 1 | 74 |
| `.dpr` | 1 | 24 |
| `.md` | 1 | 12 |

No hay `.dpk`, `.inc`, `.res` ni `.rpt`.

## Archivos de `legado/src/` y raíz

| Archivo | Tipo | Responsabilidad | Líneas |
|---|---|---|---|
| `src/UDMPrincipal.pas` | Data module | Conexión DBISAM, tablas y helpers SQL (`LeerParametro`, `EjecutarSQL`). | 100 |
| `src/UDMPrincipal.dfm` | Formulario (data module) | Componentes DBISAM: base `PEDIDOS` y tablas CLIENTES, PRODUCTOS, PEDIDOS y PEDIDO_LINEAS. | 65 |
| `src/UGlobales.pas` | Globales | Variables globales y valores por defecto (por ejemplo `gTasaIVA := 0.19;`). | 41 |
| `src/UUtilidades.pas` | Utilidades | RUT, fechas, redondeo y cierre de pedidos. | 189 |
| `src/UCalculoTotales.pas` | Unidad de lógica | Subtotal, descuento, flete, IVA y total del pedido (`CalcularTotales`). | 147 |
| `src/UStock.pas` | Unidad de lógica | Descuento de stock y registro de movimientos (`DescontarStock`). | 93 |
| `src/UFrmPrincipal.pas` | Formulario | Login, carga de parámetros y menú principal. | 104 |
| `src/UFrmPrincipal.dfm` | Formulario | Diseño del formulario principal. | 62 |
| `src/UFrmClientes.pas` | Formulario | Mantenedor de clientes y sus validaciones (`tblCliBeforePost`). | 215 |
| `src/UFrmClientes.dfm` | Formulario | Diseño del mantenedor de clientes. | 213 |
| `src/UFrmProductos.pas` | Formulario | Mantenedor de productos y filtros (`tblProdBeforePost`). | 105 |
| `src/UFrmProductos.dfm` | Formulario | Diseño del mantenedor de productos. | 98 |
| `src/UFrmPedidos.pas` | Formulario | Toma y grabado de pedidos (`btnGrabarClick`). Es la unidad más grande. | 420 |
| `src/UFrmPedidos.dfm` | Formulario | Diseño del formulario de pedidos. | 226 |
| `src/UFrmInformes.pas` | Formulario | Informe de ventas por rango de fechas y exportación a CSV. | 137 |
| `src/UFrmInformes.dfm` | Formulario | Diseño del informe; usa `DatabaseName = 'PEDIDOS'` sin `TableName`. | 160 |
| `PedidosLegado.dpr` | Programa principal | Crea `DMPrincipal` y `FrmPrincipal`. | 24 |

## Tablas DBISAM

Definidas en `legado/db/esquema-dbisam.sql` (74 líneas). Base `PEDIDOS`, directorio `C:\PEDIDOS\DATOS`, DBISAM 4.24 (`UDMPrincipal.dfm:10-12`).

| Tabla | Definición | Datos (CSV, con encabezado) | Clave y campos relevantes | Responsabilidad |
|---|---|---|---|---|
| REGIONES | `esquema:4` | 17 líneas (16 regiones) | `CODIGO`; `NOMBRE`, `FLETE_BASE` | Fletes base por región. |
| CLIENTES | `esquema:12` | 31 líneas | `ID_CLIENTE`; `RUT`, `TIPO`, `REGION`, `SALDO_VENCIDO`, `FECHA_VENCIMIENTO`, `ACTIVO` | Maestro de clientes. |
| PRODUCTOS | `esquema:29` | 41 líneas | `ID_PRODUCTO`; `CODIGO`, `CATEGORIA`, `PRECIO`, `STOCK`, `STOCK_MINIMO` | Maestro de productos y stock. |
| PEDIDOS | `esquema:43` | 61 líneas | `FOLIO`; `ID_CLIENTE`, `FECHA`, `ESTADO`, `TOTAL` | Cabecera de pedidos. |
| PEDIDO_LINEAS | `esquema:58` | 170 líneas | `FOLIO` + `LINEA`; `ID_PRODUCTO`, `CANTIDAD`, `PRECIO_UNITARIO` | Detalle de pedidos. |
| PARAMETROS | `esquema:68` | 7 líneas | `CLAVE`; `VALOR`, `DESCRIPCION` | Parámetros del sistema (IVA, pedido mínimo, etc.). |
| MOV_STOCK | **Sin definición** | Sin CSV | Campos que inserta el código: `ID_PRODUCTO`, `CANTIDAD`… | Movimientos de stock. Solo aparece en `UStock.pas:85`. |

## Lo que me llamó la atención

- `README.md` contradice el código: dice "Paradox via BDE" y "Crystal Reports 8", pero el código usa DBISAM (`UDMPrincipal.pas:6`) y no hay informes Crystal ni `.rpt`.
- `MOV_STOCK` se usa en `UStock.pas:85` pero no existe en el esquema ni en los CSV. Además, el `except` vacío de `UStock.pas:89-90` traga el error.
- La lógica de descuento está duplicada en `UCalculoTotales.pas:77-80` y `UFrmPedidos.pas:296-307`, con umbrales y operadores distintos. Queda pendiente de verificar en `reglas-negocio.md`.
- Hay una clave de base de datos escrita en el código (`UDMPrincipal.pas:41`).
