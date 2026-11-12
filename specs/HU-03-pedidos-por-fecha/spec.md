# HU-03 · Pedidos por rango de fechas

> Spec breve de la Unidad 7. La implementas con Claude Code en la etapa 2 del taller,
> en la rama `hu-03-pedidos-por-fecha`. Es un cambio chico a propósito: un PR revisable.

**Historia:** Como encargado de ventas, quiero ver solo los pedidos emitidos entre dos fechas,
para revisar lo que se vendió en un período.

## Qué se pide

Una operación en `PedidosWeb.Core` que, dada una lista de pedidos y dos fechas (`desde` y `hasta`,
de tipo `DateOnly`), devuelva solo los pedidos cuya `Fecha` está en ese rango.

Nombres sugeridos: clase `FiltroPedidos`, método `EntreFechas(pedidos, desde, hasta)`,
en `src/PedidosWeb.Core/Pedidos/`.

## Criterios de aceptación

- **CA-01** · Dado un pedido con fecha igual a `desde`, cuando filtro, entonces aparece (límite inclusive).
- **CA-02** · Dado un pedido con fecha igual a `hasta`, cuando filtro, entonces aparece (límite inclusive).
- **CA-03** · Dado un pedido fuera del rango, cuando filtro, entonces no aparece.
- **CA-04** · Dado `desde` mayor que `hasta`, cuando filtro, entonces se lanza `ArgumentException`
  con un mensaje que explique el problema.

## Fuera de alcance

- Exponer la ruta en la API.
- Cambiar `IPedidoRepositorio` o los pedidos de ejemplo.
