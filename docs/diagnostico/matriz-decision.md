# Matriz de decisión de Pedidos Legado

Los pesos y la matriz fueron propuestos a partir de `riesgos.md`, `reglas-negocio.md` y `dependencias.md`, y aprobados por la persona responsable. Los puntajes de Personas son una suposición, porque no hay datos del equipo.

**Escala:** de 1 a 5, donde 5 es lo mejor en ese criterio (en Costo, lo más barato; en Riesgo, lo menos riesgoso).

**Estrategias:**
- **Mantener:** dejar el Delphi como está.
- **Refactorizar:** ordenar el Delphi sin cambiar de plataforma.
- **Modernizar:** reescribir la lógica en .NET (`PedidosWeb.Core`) conservando las reglas.
- **Reemplazar:** sustituirlo por una solución nueva con reglas nuevas.

## Pesos

| Criterio | Qué mide | Peso |
|---|---|---|
| Costo | Cuánto cuesta en horas y dinero | 20 % |
| Complejidad | Qué tan difícil es técnicamente | 15 % |
| Riesgo | Qué puede salir mal en el camino | 30 % |
| Impacto | Cuánto valor entrega al negocio | 25 % |
| Personas | Conocimiento disponible en el equipo | 10 % |
| **Total** |  | **100 %** |

## Matriz

Puntaje ponderado de cada estrategia (puntaje 1–5 por criterio × peso).

| Módulo | Mantener | Refactorizar | Modernizar | Reemplazar | Recomendación |
|---|---|---|---|---|---|
| Cálculo de totales y descuentos | 2,70 | 3,05 | **3,95** | 2,05 | Modernizar |
| Clientes y RUT | 3,00 | 3,05 | **3,55** | 2,55 | Modernizar |
| Pedidos (toma y grabado) | 2,70 | 2,95 | **3,45** | 2,15 | Modernizar |
| Stock y movimientos | 2,70 | 2,75 | **3,25** | 2,70 | Modernizar |
| Acceso a datos (DBISAM) | 2,70 | 3,05 | **3,55** | 3,10 | Modernizar |
| Informes | **3,55** | 3,20 | 3,00 | 3,35 | Mantener (postergar) |

### Puntajes por criterio

Orden: Costo / Complejidad / Riesgo / Impacto / Personas.

| Módulo | Mantener | Refactorizar | Modernizar | Reemplazar |
|---|---|---|---|---|
| Cálculo de totales | 5/5/1/1/4 | 4/3/3/2/4 | 3/4/4/5/3 | 1/2/2/3/2 |
| Clientes y RUT | 5/5/2/1/4 | 4/3/3/2/4 | 3/3/4/4/3 | 2/2/3/3/2 |
| Pedidos | 5/5/1/1/4 | 3/2/3/3/4 | 2/2/4/5/3 | 1/1/2/4/2 |
| Stock | 5/5/1/1/4 | 4/3/2/2/4 | 3/3/3/4/3 | 2/3/3/3/2 |
| Datos | 5/5/1/1/4 | 4/3/3/2/4 | 3/3/4/4/3 | 2/2/4/4/2 |
| Informes | 5/5/3/2/4 | 4/4/3/2/4 | 3/3/3/3/3 | 2/3/4/4/3 |

Ejemplo de cuenta: Cálculo de totales con Modernizar = 3×0,20 + 4×0,15 + 4×0,30 + 5×0,25 + 3×0,10 = 3,95.

### Qué respalda cada puntaje

- **Cálculo de totales:** la lógica está separada de la interfaz y de la base de datos (`UCalculoTotales` solo usa `SysUtils`). Mantenerla deja el descuento duplicado e inconsistente (RS-04).
- **Clientes y RUT:** validación del RUT duplicada con el dígito verificador invertido (RS-07).
- **Pedidos:** es la unidad más grande (420 líneas) y mezcla interfaz, SQL y reglas, sin transacción (RS-03, RS-05).
- **Stock:** `MOV_STOCK` no tiene definición y el error se traga (RS-08). El riesgo de Modernizar es 3 por esa incertidumbre.
- **Acceso a datos:** clave en el código y SQL concatenado (RS-01, RS-02). El proyecto nuevo guarda los datos en memoria, así que «modernizar» significa pasar a repositorios en `PedidosWeb.Core`.
- **Informes:** menor impacto y sin reglas críticas. Mantener gana por 0,20 sobre Reemplazar.

### Orden de ataque que se desprende

1. Cálculo de totales.
2. Clientes y RUT.
3. Pedidos.
4. Stock.
5. Acceso a datos.
6. Informes, al final.

## Resumen ejecutivo

1. **Qué es y su estado:** sistema de pedidos en Delphi con DBISAM, 25 archivos y 2.812 líneas, con README desactualizado.
2. **Hallazgo principal (con evidencia):** el descuento se calcula en dos lugares con umbrales distintos (`UCalculoTotales.pas:77-80` y `UFrmPedidos.pas:296-301`).
3. **Riesgo más urgente:** clave de la base de datos en el código y SQL sin escapar (`UDMPrincipal.pas:41`, `UFrmPedidos.pas:259-262`).
4. **Estrategia y por dónde empezar:** modernizar el núcleo en .NET empezando por el cálculo de totales, y dejar los informes para el final.
5. **Próximo paso que se pide aprobar:** esta matriz y la spec de la primera historia, el cálculo de totales.
