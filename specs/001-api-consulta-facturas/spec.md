# Spec 001 — API de consulta de facturas

| Campo | Valor |
|---|---|
| Estado | Implementada |
| Origen | [enunciado.md](enunciado.md) |
| Plan | [plan.md](plan.md) |
| Tareas | [tasks.md](tasks.md) |
| Última revisión | 2026-10-26: se remueve JWT (RNF-03) |

Esta spec describe **qué** debe hacer el sistema y **por qué**, sin decisiones de implementación (ver [plan.md](plan.md)).

## Contexto

Se dispone de un JSON con facturas emitidas (vendedor, comprador, comuna, detalle de productos) que no incluye totales calculados. Se requiere un servicio que permita consultarlas y obtener agregados por comprador y por comuna.

## Historias de usuario

### HU-1 — Listar facturas con su total
Como consumidor de la API, quiero obtener todas las facturas con su total calculado, para conocer el monto real de cada una.

- **Dado** el JSON de facturas, **cuando** pido la lista, **entonces** recibo todas las facturas y cada una trae `TotalFactura` = Σ (`CantidadProducto` × `Valor`) de su detalle, y cada línea su `TotalProducto`.
- Los totales presentes en el JSON se ignoran.

### HU-2 — Facturas de un comprador
Como consumidor, quiero consultar las facturas de un comprador por su RUT.

- Acepta `12345678-9`, `12.345.678-K`, `12345678K` o solo el número (DV calculado); el DV es insensible a mayúsculas.
- RUT mal formado o con DV incorrecto → 400.
- RUT válido sin facturas → 404.

### HU-3 — Comprador con mayor compra
Como consumidor, quiero saber qué comprador acumula el mayor monto total en compras.

- Devuelve un único comprador con su monto acumulado y cantidad de facturas.
- Sin facturas en la fuente → 404.

### HU-4 — Compradores con monto total
Como consumidor, quiero la lista de compradores con el monto total de sus compras.

- Un elemento por comprador distinto, con monto total y cantidad de facturas, ordenado de mayor a menor monto.

### HU-5 — Facturas agrupadas por comuna
Como consumidor, quiero ver las facturas agrupadas por comuna del comprador.

- Un grupo por comuna, con cantidad de facturas, monto total y las facturas.

### HU-6 — Facturas de una comuna
Como consumidor, quiero obtener las facturas de una comuna específica.

- Código de comuna ≤ 0 → 400.
- Comuna sin facturas → 404.

## Requisitos funcionales

| Id | Requisito | HU |
|---|---|---|
| RF-01 | Listar todas las facturas con totales de factura y línea calculados en ejecución | HU-1 |
| RF-02 | Filtrar facturas por RUT de comprador, validando formato y DV (módulo 11) | HU-2 |
| RF-03 | Obtener el comprador con mayor monto acumulado | HU-3 |
| RF-04 | Listar compradores con monto total, orden descendente | HU-4 |
| RF-05 | Agrupar facturas por comuna del comprador | HU-5 |
| RF-06 | Filtrar facturas por código de comuna | HU-6 |

## Requisitos no funcionales

| Id | Requisito | Fuente |
|---|---|---|
| RNF-01 | El JSON entregado es la única fuente de datos y no se modifica su estructura | Enunciado |
| RNF-02 | Todos los métodos protegidos con ApiKey | Enunciado |
| ~~RNF-03~~ | ~~Autorización por JWT en cada endpoint de negocio~~ — **Removido** el 2026-10-26, ver [D-07](plan.md#d-07--remoción-de-jwt) | ADR |
| RNF-04 | Códigos HTTP adecuados; errores en formato ProblemDetails | Enunciado |
| RNF-05 | Patrón de arquitectura (Clean Architecture) | Enunciado |
| RNF-06 | .NET 10 con Minimal API (cumple ".NET 8 o superior") | Enunciado + ADR |
| RNF-07 | Compila y se ejecuta correctamente | Enunciado |

## Fuera de alcance

- Altas, modificaciones o eliminación de facturas.
- Paginación, ordenamiento configurable o filtros combinados.
- Autenticación de usuarios o clientes más allá de la ApiKey (JWT, OAuth, etc.).

## Ambigüedades resueltas

| Pregunta | Decisión |
|---|---|
| ¿ApiKey o JWT? El enunciado pide ApiKey y el ADR pide JWT | Inicialmente ambos ([D-03](plan.md#d-03--apikey--jwt-reemplazada-por-d-07)). Desde el 2026-10-26, solo ApiKey, como pide el enunciado ([D-07](plan.md#d-07--remoción-de-jwt)) |
| ¿Qué pasa si hay empate en mayor compra? | Se devuelve uno solo (el primero según el orden descendente) |
| ¿Una consulta sin resultados es 200 con lista vacía o 404? | 404 en búsquedas por clave (RUT, comuna); 200 en listados generales |
