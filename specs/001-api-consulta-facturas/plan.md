# Plan técnico — Spec 001

Describe **cómo** se implementa [spec.md](spec.md), respetando la [constitución](../../.specify/memory/constitution.md).

## Stack

| Elemento | Elección |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal API, `TypedResults` |
| Seguridad | Endpoint filter de ApiKey |
| Documentación | `Microsoft.AspNetCore.OpenApi` + Swagger UI (solo en Development) |
| Errores | `AddProblemDetails`, `UseExceptionHandler`, `UseStatusCodePages` |
| Pruebas | xUnit + `WebApplicationFactory` |

## Estructura

```
src/
  Facturas.Domain          Factura, DetalleFactura, Producto, Rut (módulo 11)
  Facturas.Application     IFacturaService / FacturaService, DTOs, puerto IFacturaRepository
  Facturas.Infrastructure  JsonFacturaRepository (lee el JSON una vez y lo deja en memoria)
  Facturas.Api             Endpoints, seguridad, OpenAPI
tests/
  Facturas.Tests           Domain / Application / Api
```

Modelo de datos: [data-model.md](data-model.md). Contrato HTTP: [contracts/api.md](contracts/api.md).

## Decisiones

### D-01 — Clean Architecture en 4 proyectos
Cumple RNF-05 con dependencias hacia adentro. El repositorio JSON queda detrás de un puerto, de modo que cambiar la fuente no toca dominio ni casos de uso.

### D-02 — Totales como propiedades calculadas del dominio
`DetalleFactura.Total` y `Factura.Total` son propiedades derivadas, así que no se pueden desincronizar. Los totales del JSON no se mapean al dominio (RNF-01, constitución II).

### D-03 — ApiKey + JWT (reemplazada por D-07)
> **Reemplazada.** Se conserva como registro histórico. La ApiKey sigue vigente tal como se describe aquí; la parte de JWT fue removida en [D-07](#d-07--remoción-de-jwt).

- El enunciado exige ApiKey y el ADR exige JWT, así que se aplican ambos:
  - La ApiKey va en un endpoint filter sobre el grupo `/api/v1`, incluido `/auth/token`, y se compara en tiempo constante sobre hashes SHA-256.
  - El JWT se emite en `POST /auth/token` con `clientId`/`clientSecret` configurados y exige el scope `facturas:read` mediante una policy.
- Ausencia o invalidez de cualquiera de los dos → 401. JWT sin scope → 403.

### D-04 — Rut como value object
Centraliza el parseo de los formatos aceptados y la validación del DV, de modo que un RUT inválido nunca llega al caso de uso (→ 400).

### D-05 — Configuración validada al arranque
`ValidateOnStart` sobre la ApiKey. El valor de desarrollo va en `appsettings.Development.json`; en otros entornos se usan variables de entorno.

### D-06 — DTOs con la misma forma que el JSON
`FacturaDto` replica los nombres del JSON fuente (`RUTComprador`, `DetalleFactura`, ...) con los totales ya calculados, para que el consumidor reconozca la estructura original.

### D-07 — Remoción de JWT
**Fecha:** 2026-10-26 · **Estado:** Aceptada · **Reemplaza:** la parte JWT de D-03

**Contexto.** El enunciado pide proteger todos los métodos con ApiKey. El JWT se había agregado por una indicación del ADR que no forma parte de los requisitos del ejercicio. Mantener ambos obligaba al consumidor a dos pasos (obtener token y luego llamar) y sumaba configuración (clave de firma, clientes, scopes) que el enunciado no pide evaluar.

**Decisión.** Se usa solo ApiKey en todo `/api/v1`.

**Qué se removió:**
- Endpoint `POST /api/v1/auth/token` (`AuthEndpoints.cs`).
- `TokenService`, `JwtOptions`, `ApiClientOptions` y `Policies` (policy `facturas:read`).
- Configuración de autenticación y autorización JWT en `SecurityExtensions` y `UseAuthentication`/`UseAuthorization` en `Program.cs`.
- `RequireAuthorization` de los grupos `/facturas` y `/compradores`.
- Paquete `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Secciones `Jwt` y `ApiClients` de `appsettings*.json`.
- Esquema Bearer en `SecuritySchemesTransformer` y el header `Authorization` en `Facturas.Api.http`.
- Pruebas: `Sin_jwt_responde_401`, `Credenciales_invalidas_responde_401_y_faltantes_400`, `Jwt_sin_scope_responde_403`. `ApiKey_invalida_responde_401_aun_con_jwt_valido` pasó a ser `ApiKey_invalida_responde_401`, y `Sin_ApiKey_responde_401` ahora prueba `GET /facturas`.

**Consecuencias.**
- Ya no hay respuesta 403; el 401 queda solo para ApiKey ausente o inválida.
- Se pierde la expiración de credenciales y los permisos por scope. Si en el futuro se necesitan, se deberá abrir una nueva spec en lugar de revertir este cambio.

### D-08 — `record` para datos inmutables, `class` para la entidad
**Fecha:** 2026-10-26 · **Estado:** Aceptada

**Contexto.** Factura, detalle y producto aparecen en tres modelos (lectura del JSON, dominio, DTO de salida), lo que a primera vista parece duplicación.

**Decisión.**
- Se mantienen los tres modelos, porque cada uno cambia por un motivo distinto:
  - **Lectura:** el JSON trae enteros como `1569.0`, que System.Text.Json no convierte a `long`.
  - **Dominio:** contiene las reglas (totales calculados, `Rut`).
  - **DTO:** fija el contrato HTTP. Sin él, `Rut` se serializaría como `{ numero, dv }`.
- Se usa `record` en todo lo que es dato inmutable comparado por valor: DTOs, modelos de lectura y los objetos de valor `DetalleFactura`, `Producto` y `Rut`. Así cada uno se define en una línea y se compara por valor; por ejemplo, `GroupBy(f => f.Comprador)` agrupa por `Rut`.
- `Factura` es `class` porque es una entidad: su identidad es `NumeroDocumento` y protege su colección de detalle.

**Simplificaciones aplicadas en la misma revisión:**
- Los modelos de lectura ya no declaran `TotalFactura` / `TotalProducto`.
- `JsonFacturaRepository` carga el archivo en el constructor (es singleton) en lugar de usar `Lazy<Task<...>>`.
- `GetCompradoresConTotalAsync` agrupa por `Rut` y arma el DTO en línea, sin método auxiliar.
- `CompradorTotalDto` ya no incluye el campo `Rut` (texto `"12345678-9"`), que repetía `RUTComprador` + `DvComprador`. **Cambia la respuesta** de `/compradores` y `/compradores/mayor-compra`.

## Cumplimiento de la constitución

| Principio | Cómo se cumple |
|---|---|
| I | `JsonFacturaRepository` es la única implementación de `IFacturaRepository` |
| II | D-02 |
| III | ApiKey de D-03, D-05, D-07 |
| IV | `TypedResults.Problem`, ProblemDetails global |
| V | D-01 |
| VI | Ver la trazabilidad en [tasks.md](tasks.md) |
