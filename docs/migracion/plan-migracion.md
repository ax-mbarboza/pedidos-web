# Plan de migración · Pedidos Legado → Pedidos Web

Estrategia: **modernizar** módulo a módulo (strangler fig), según `docs/diagnostico/matriz-decision.md`.

## 1. Etapas
1. **Cálculo de totales** en C# (`src/PedidosWeb.Core/Totales/`): hecho en la Unidad 5. 60 casos del golden master en verde; defectos en `defectos-corregidos.md`.
2. **Clientes y RUT:** una sola validación, la estándar (`ValidaRut`). `RutOk` no se migra (RS-07).
3. **Pedidos:** toma y grabado con transacción y folio asignado por el repositorio (RS-03, RS-06). Aquí se decide el umbral del descuento (H1).
4. **Stock:** antes de empezar, confirmar si `MOV_STOCK` existe en producción (RS-08).
5. **Acceso a datos:** repositorios en `PedidosWeb.Core`, sin clave en el código ni SQL concatenado (RS-01, RS-02).
6. **Informes:** al final y solo si hace falta; la matriz recomienda mantenerlos (RS-09 queda pendiente).

## 2. Convivencia
- El legado sigue siendo el sistema de registro hasta el corte de cada etapa. El portal calcula en paralelo y no graba.
- Los resultados se comparan con `migracion/golden-master.csv` (el contrato) y, con los pedidos reales del período, total contra total.
- Toda diferencia queda explicada en `docs/migracion/defectos-corregidos.md`. Hoy hay dos: el redondeo (D1, ±2 pesos por pedido) y los umbrales (H1, sin corregir).
- Contabilidad concilia con una tolerancia de ±2 pesos por pedido mientras dure la convivencia.

## 3. Riesgos
| Riesgo | Mitigación |
|---|---|
| Totales distintos entre legado y portal (redondeo D1; umbrales H1, RS-04). | Golden master como contrato, cada diferencia registrada y conciliación de Contabilidad con tolerancia. Decidir H1 antes de la etapa 3. |
| El golden master salió de una réplica de `UCalculoTotales.pas`, no del sistema en ejecución. | Antes del corte, capturar los valores ejecutando el legado real y repetir la comparación. |
| La regla del redondeo cambió pese a que `reglas-negocio.md` dice que Contabilidad la exige. | Confirmar D1 con Contabilidad antes de apagar el cálculo del legado. |
| Pedidos sin transacción, con folio `MAX + 1` y stock que no se descuenta (RS-03, RS-05, RS-06). | Resolverlos al migrar Pedidos y Stock, con tests; no replicar esos defectos. |
| Clave de la base y SQL sin escapar pasan al código nuevo (RS-01, RS-02). | Sin secretos en el repositorio (`AGENTS.md`) y consultas parametrizadas. |

## 4. Criterio de corte
- **Se apaga el cálculo de totales del legado** cuando: los 60 casos pasan, la comparación con pedidos reales de un ciclo completo de cierre (propuesta, a confirmar con Contabilidad) solo muestra diferencias ya registradas y dentro de ±2 pesos, y Contabilidad firma D1.
- **Los demás módulos** siguen la misma regla: tests en verde, una persona leyó el diff y el negocio aprobó las diferencias.
- **Vuelta atrás:** el legado no se desinstala en la etapa de corte. Si algo falla, se vuelve a usar el total del legado y se corrige en el portal sin perder pedidos.
