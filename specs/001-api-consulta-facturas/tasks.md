# Tareas — Spec 001

`[x]` hecho · `[ ]` pendiente. Cada tarea indica el requisito que cubre y la prueba que lo verifica.

## Fase 1 — Base

- [x] T01 Solución .NET 10 con proyectos Domain / Application / Infrastructure / Api / Tests (RNF-05, RNF-06)
- [x] T02 Copiar `Ejemplo.json` sin modificar a `src/Facturas.Api/Data` (RNF-01)

## Fase 2 — Dominio

- [x] T03 `Rut` con parseo multi-formato y validación módulo 11 → `RutTests.TryParse_formatos_validos`, `RutTests.TryParse_rechaza_entradas_invalidas` (RF-02)
- [x] T04 Entidades con totales calculados (RF-01)

## Fase 3 — Infraestructura y casos de uso

- [x] T05 `JsonFacturaRepository` que lee el JSON una vez e ignora los totales (RNF-01)
- [x] T06 `FacturaService.GetFacturasAsync` → `FacturaServiceTests.GetFacturas_calcula_el_total_desde_el_detalle` (RF-01)
- [x] T07 `GetFacturasPorCompradorAsync` → `FacturaServiceTests.GetFacturasPorComprador_filtra_por_rut` (RF-02)
- [x] T08 `GetCompradoresConTotalAsync` / `GetCompradorConMayorCompraAsync` → `FacturaServiceTests.GetCompradores_suma_totales_y_ordena_descendente` (RF-03, RF-04)
- [x] T09 Agrupación y filtro por comuna → `FacturaServiceTests.Agrupacion_y_filtro_por_comuna` (RF-05, RF-06)

## Fase 4 — API y seguridad

- [x] T10 Endpoints según [contracts/api.md](contracts/api.md) → `FacturasApiTests.Endpoints_responden_codigo_http_esperado`, `GetFacturas_responde_200_con_totales_calculados` (RF-01..06, RNF-04)
- [x] T11 Filtro ApiKey en `/api/v1` → `Sin_ApiKey_responde_401`, `ApiKey_invalida_responde_401` (RNF-02)
- [x] ~~T12 Emisión y validación de JWT con scope (RNF-03)~~ — revertida por T16
- [x] T13 ProblemDetails global y OpenAPI + Swagger UI (RNF-04)

## Fase 5 — Cierre

- [x] T14 README con arquitectura, seguridad, endpoints y ejecución
- [x] T15 Verificar `dotnet build` y `dotnet test` en verde antes de entregar (RNF-07): 0 advertencias, 30/30 pruebas el 2026-10-26

## Fase 6 — Ajuste al enunciado (2026-10-26)

- [x] T16 Remover JWT y sus pruebas; dejar solo ApiKey. Detalle en [D-07](plan.md#d-07--remoción-de-jwt) (RNF-02)
- [x] T17 Actualizar README, constitución, spec y contrato

## Fase 7 — Legibilidad (2026-10-26)

- [x] T18 Simplificaciones de [D-08](plan.md#d-08--record-para-datos-inmutables-class-para-la-entidad): modelos de lectura sin totales, carga del JSON en el constructor, agrupación por `Rut`, `CompradorTotalDto` sin campo redundante. Build sin advertencias, 30/30 pruebas
- [x] T19 Corregir en [data-model.md](data-model.md) el alcance de la validación del DV

## Matriz de trazabilidad

| Requisito | Tareas |
|---|---|
| RF-01 | T04, T06, T10 |
| RF-02 | T03, T07, T10 |
| RF-03 | T08, T10 |
| RF-04 | T08, T10 |
| RF-05 | T09, T10 |
| RF-06 | T09, T10 |
| RNF-01 | T02, T05 |
| RNF-02 | T11, T16 |
| ~~RNF-03~~ | Removido (T16) |
| RNF-04 | T10, T13 |
| RNF-05 | T01 |
| RNF-06 | T01 |
| RNF-07 | T15 |
