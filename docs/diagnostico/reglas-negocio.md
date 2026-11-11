# Reglas de negocio de Pedidos Legado

Las rutas son relativas a `legado/src/`. «Verificada: Sí» significa que leí el código en esa línea. Donde una regla está en más de un lugar, anoto todos y señalo las diferencias. Las reglas con dos valores distintos quedan como pregunta abierta: no decido cuál rige.

| # | Regla | Archivo:línea | Condición | Efecto | Verificada |
|---|---|---|---|---|---|
| 1 | Descuento por volumen (versión `UCalculoTotales`) | `UCalculoTotales.pas:77-80` | Base de descuento **mayor que** 1.000.000 o **mayor que** 500.000 | 8% o 5% de descuento. **No rige:** difiere de la regla 2 en el valor exacto del umbral y se descarta como referencia. | Sí |
| 2 | Descuento por volumen (versión `UFrmPedidos`, la que se guarda). **Regla vigente** (decisión de la persona responsable) | `UFrmPedidos.pas:296-301` | Base **mayor o igual que** 1.000.000 o 500.000 | 8% o 5% de descuento. **Difiere de la regla 1 en el valor exacto del umbral.** | Sí |
| 3 | La base del descuento excluye la categoría `OFE` | `UCalculoTotales.pas:70`; `UFrmPedidos.pas:287` | Línea con categoría `OFE` | No suma a la base del descuento (sí al subtotal) | Sí |
| 4 | Cliente VIP: +3% de descuento y tope de 10% | `UCalculoTotales.pas:88-91`; `UFrmPedidos.pas:302-307` | Cliente VIP | Suma 3 puntos y el total nunca pasa de 10% | Sí |
| 5 | Descuento de Navidad +2% | `UFrmPedidos.pas:304-305` | `gDescNavidad` verdadero y mes de la fecha del pedido = 12 | Suma 2 puntos. Solo está en `UFrmPedidos`, no en `UCalculoTotales`. | Sí |
| 6 | Flete con recargo en regiones 11 y 12 | `UCalculoTotales.pas:97-98`; `UFrmPedidos.pas:312-313` | Región 11 o 12 | Flete base × 1,15, redondeado | Sí |
| 7 | Sin líneas, no hay flete | `UCalculoTotales.pas:116-117` | Pedido sin líneas | Flete = 0 | Sí |
| 8 | IVA sobre neto más flete | `UCalculoTotales.pas:105`; `UFrmPedidos.pas:318` | Siempre | IVA = (neto + flete) × tasa, redondeado. En el informe se usa 0,19 fijo (`UFrmInformes.pas:92`). | Sí |
| 9 | Redondeo al peso | `UCalculoTotales.pas:50`; `UUtilidades.pas:30` | Descuento, flete e IVA | `Trunc(valor + 0,49)`. Redondea hacia abajo desde 0,51, no es el redondeo habitual. | Sí |
| 10 | Pedido mínimo | `UFrmPedidos.pas:231-234` | Subtotal menor que 30.000 y región distinta de 13 | Rechaza el pedido. PARAMETROS dice 25.000 pero no se lee. | Sí |
| 11 | Cierre de pedidos a las 18:00 | `UUtilidades.pas:128-140`; `UFrmPedidos.pas:240-243` | Hora de 18 o más | Fecha pasa al siguiente día hábil (lunes a viernes, sin feriados) | Sí |
| 12 | Stock no negativo; VIP puede deber hasta 20 | `UStock.pas:40-50` | Stock resultante menor que 0 (menor que -20 si es VIP) | No descuenta ese producto | Sí |
| 13 | Validación de RUT, módulo 11 | `UUtilidades.pas:72-79`; `UFrmClientes.pas:192-199` | Alta de cliente (`UFrmClientes`) y pedido (`UFrmPedidos:91`) | **Rige `ValidaRut` (estándar chileno).** `RutOk` se descarta por tener el dígito verificador invertido: ver abajo | Sí |
| 14 | Solo jefatura crea clientes VIP | `UFrmClientes.pas:143-147` | Tipo `V` y `gNivelUsuario` menor que 2 | Rechaza el guardado. El nivel 1 es el valor inicial (`UGlobales.pas:30`) y solo se asigna 9 (`UFrmPrincipal.pas:50`). | Sí |
| 15 | Un cliente con pedidos no se elimina; región entre 1 y 16 | `UFrmClientes.pas:107-111`, `:148-153` | Intento de eliminar o guardar | Rechaza la operación. Cliente nuevo parte con tipo `N` y región 13 (`:90-91`). | Sí |

## Diferencias entre implementaciones

- **RUT (regla 13):** con resto 11, `ValidaRut` espera `0` y `RutOk` espera `K`. Con resto 10, `ValidaRut` espera `K` y `RutOk` espera `0`. El estándar chileno es 11 → `0` y 10 → `K`, por lo que `RutOk` es la que está invertida. Rige `ValidaRut`, por decisión de la persona responsable.
- **Descuento (reglas 1, 2 y 5):** con base exactamente 500.000 o 1.000.000, `UFrmPedidos` aplica el descuento y `UCalculoTotales` no. El total guardado (`UFrmPedidos.pas:320`) viene de `UFrmPedidos`. **Rige `UFrmPedidos`** (`>=` con bandas de 500.000 y 1.000.000), por decisión de la persona responsable.
- **Navidad (regla 5):** `gDescNavidad` nace en `False` (`UGlobales.pas:33`) y ninguna rama lo cambia, así que hoy el +2% no se aplica. El parámetro `DESC_NAVIDAD` de PARAMETROS vale `N` y tampoco se carga.

## Preguntas abiertas

1. Rige `UFrmPedidos`: `>=` con bandas de 500.000 (5%) y 1.000.000 (8%). El 400.000 del comentario de `UCalculoTotales.pas:4` no es la regla vigente.
2. El pedido mínimo es 30000
3. Rige el estándar chileno (`ValidaRut`, `UUtilidades.pas:72-79`). `RutOk` (`UFrmClientes.pas:192-199`) está invertida y no se migra.
4. La promoción de navidad no sigue vigente
5. Las regiones 11 y 12 son reglas, añadilas como tal
6. `Trunc(x + 0,49)` -> Contabilidad lo exige
7. Se mantiene lo que dicta el código: ninguna línea asigna el nivel 2 (`UGlobales.pas:30`, `UFrmPrincipal.pas:50`). Solo el nivel 9 (admin) puede crear clientes VIP (`UFrmClientes.pas:143`). El comentario de `UGlobales.pas:13` menciona el nivel 2 (supervisor), pero el código no lo asigna.
8. El corte de las 18:00 sigue vigente y se mantiene el comportamiento del código (regla 11). **Brecha conocida:** el legado no considera feriados, solo salta sábado y domingo (`UUtilidades.pas:122-125`, `:133-138`). Se mantiene así. Los pedidos antes de las 18:00 quedan con la fecha del día, sea hábil o no.
