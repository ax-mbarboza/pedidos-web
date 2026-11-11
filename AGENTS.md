# AGENTS.md

## Proyecto
Portal web "Pedidos": API REST en ASP.NET Core (.NET 10), modernización de un sistema legado en Delphi.
Los datos son ficticios y viven en memoria (sin base de datos).

## Comandos
- Compilar: `dotnet build`
- Testear: `dotnet test`
- Ejecutar la API: `dotnet run --project src/PedidosWeb.Api`

## Estilo de código
- Nombres de tipos, métodos y propiedades en español (`Pedido`, `ObtenerPorId`, `ClienteId`).
- Dinero: `decimal` en CLP, sin decimales. Nunca `double` ni `float`.
- Fechas: `DateOnly` (hora local de Chile, sin hora).
- `PedidosWeb.Core`: lógica de dominio y repositorios. `PedidosWeb.Api`: solo HTTP (rutas, parámetros, códigos de respuesta).

## Tests
- xUnit, en `tests/PedidosWeb.Core.Tests`, espejando la carpeta de Core (`Pedidos/`, `Folios/`).
- Nombre: `Metodo_Escenario_ResultadoEsperado`.
- Los tests se ejecutan solo antes de cada commit y luego de cada cambio finalizado.

## Seguridad
- Nunca secretos ni claves en el código.

## Reglas de trabajo
- Toda funcionalidad parte de una spec aprobada en `specs/<historia>/spec.md`.
- Un test xUnit por criterio de aceptación.
- Nada de commit, push ni merge: eso lo hace siempre una persona, después de leer el diff.
- Sin preguntar, solo puedo leer archivos y ejecutar `dotnet build` / `dotnet test`. Todo cambio de archivos requiere aprobación.
- Plan antes de editar: antes de modificar un archivo, explico mi plan en máximo 5 viñetas y espero la aprobación.
- Si algo no está en la spec, propongo una opción, la anoto en "Preguntas abiertas" y espero el OK. No asumo.
- Si una regla del negocio no está escrita, pregunto. No la invento ni la supongo.

## Reglas del equipo
- Idioma: respondo, comento el código y escribo los mensajes en español de Chile. Las clases y los métodos del dominio van en español (`FormatearFolio`, `CalcularTotal`).
- Cada cambio de comportamiento lleva su test xUnit. Si cambia una regla, el test que la prueba cambia en el mismo cambio.
- No agrego paquetes NuGet sin preguntar primero.
- Solo trabajo en `src/`, `tests/`, `docs/`, `specs/`, `legado/`, `.claude/`, `migracion/`. No toco la configuración del IDE (`.idea/`), ni `AGENTS.md`, ni archivos fuera del repositorio.

## Definition of Done
- Compila sin warnings nuevos.
- `dotnet test` en verde.
- Cada regla del README tocada tiene su test.
- Una persona leyó y entendió el diff.
