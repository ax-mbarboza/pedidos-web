# Defectos corregidos · migración del cálculo de totales

## D1 · R05 (regla 9) · Redondeo al peso (CORREGIDO)

- **Qué hace el legado:** `Calc3` hace `Trunc(valor + 0.49)` (`legado/src/UCalculoTotales.pas:50`; la copia `RedondeaPesos` está en `UUtilidades.pas:30`). Solo sube desde la fracción 0,51, así que el 0,50 exacto **baja**. Se aplica al descuento (`:114`), al flete (`:98`, `:100`) y al IVA (`:105`).
- **Corrección:** `Redondear` en `src/PedidosWeb.Core/Totales/CalculoTotales.cs` usa `Math.Round(valor, MidpointRounding.AwayFromZero)`: el 0,50 exacto sube. Decisión de la persona responsable de la migración.
- **Por qué:** el redondeo queda como el comercial habitual y deja de depender de `+ 0.49`. Efecto en el cliente: entre −2 y +2 pesos por pedido.
- **Casos del golden master que cambian (13 de 60):**

| Caso | Campos que cambian (antes → después) |
|---|---|
| 2 | IVA 36.964 → 36.965; total 231.514 → 231.515 |
| 6 | descuento 25.767 → 25.768; neto 872.053 → 872.052; total 1.041.908 → 1.041.907 |
| 8 | IVA 100.120 → 100.121; total 627.070 → 627.071 |
| 18 | flete 18.963 → 18.964; total 180.360 → 180.361 |
| 21 | flete 17.238 → 17.239; IVA 248.778 → 248.779; total 1.558.138 → 1.558.140 |
| 23 | flete 18.963 → 18.964; total 599.716 → 599.717 |
| 25 | descuento 46.425 → 46.426; neto 2.153.845 → 2.153.844; IVA 410.542 → 410.541; total 2.571.287 → 2.571.285 |
| 27 | flete 17.238 → 17.239; total 1.510.417 → 1.510.418 |
| 36 | flete 18.963 → 18.964; total 1.833.007 → 1.833.008 |
| 37 | flete 18.963 → 18.964; total 724.114 → 724.115 |
| 42 | flete 18.963 → 18.964; total 1.153.066 → 1.153.067 |
| 46 | flete 17.238 → 17.239; total 1.708.835 → 1.708.836 |
| 54 | flete 17.238 → 17.239; total 704.391 → 704.392 |

- **Suma de la columna total:** 58.202.135 → 58.202.144 (+9).
- **Commit:** `fix(totales): redondeo al peso con mitad hacia arriba (R05)`, separado del «igual al legado».
- **Convivencia:** mientras el legado siga truncando, un mismo pedido puede diferir en 1 o 2 pesos entre ambos sistemas (flete de las regiones 11 y 12, IVA y descuento con fracción exacta de 0,50). Contabilidad debe conciliar con una tolerancia de ±2 pesos por pedido hasta apagar el legado.
- **Pendiente:** `docs/diagnostico/reglas-negocio.md` (pregunta abierta 6) dice que Contabilidad exige `Trunc(x + 0,49)`. Esta corrección lo contradice y hay que confirmarla con Contabilidad. Ese documento no se modificó.

## H1 · Umbrales del descuento por volumen (HALLAZGO, NO CORREGIDO)

- **Qué encontraste:** `UCalculoTotales.pas:77` y `:79` usan `>` (más de 1.000.000 y más de 500.000). `UFrmPedidos.pas:296` y `:298` usan `>=`. `reglas-negocio.md` declara vigente la de `UFrmPedidos`; el total que se guarda sale de ahí (`UFrmPedidos.pas:320`). Es el riesgo RS-04.
- **Efecto:** con una base de descuento exactamente en el umbral, el legado calcula con `>` y la regla vigente con `>=`. En el golden master son 13 casos: base 500.000 (casos 8, 11, 19, 23, 50, 55), que pasarían de 0% a 5% o de 3% a 8% si es VIP; y base 1.000.000 (casos 26, 32, 35, 39, 42, 48, 60), que pasarían de 5% a 8%. El total de cada caso bajaría unos 29.750 pesos en el primer grupo y unos 35.700 en el segundo. La suma de la columna total bajaría unos 428.400 pesos (cálculo hecho antes de aplicar R05, sobre los valores del legado).
- **Quién decide:** Comercial, con Contabilidad. El C# replica `UCalculoTotales` (`>`) y el golden master conserva esos valores. Queda pendiente aplicar `>=` en un cambio aparte, con su registro de casos.
