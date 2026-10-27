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

### D-01 — Clean Architecture en 4 proyectos, en versión liviana
**Fecha:** 2026-10-26 · **Estado:** Aceptada · **Revisada:** 2026-10-27

**Contexto.** El enunciado pide "algún patrón de desarrollo (Ej. Clean Architecture)" y que el JSON funcione como "simulación de base de datos". El problema en sí es pequeño: 6 consultas de solo lectura sobre un archivo estático, sin escrituras, cuyas únicas reglas de negocio son la suma de totales y la validación del RUT.

**Decisión.** Clean Architecture con 4 proyectos (Domain, Application, Infrastructure, Api) y dependencias hacia adentro. Se eligió porque:
- Es el patrón que el enunciado nombra, así que quien evalúa lo reconoce de inmediato.
- El puerto `IFacturaRepository` permite reemplazar el JSON por una base de datos real sin tocar el dominio ni los casos de uso. Ese es el escenario que sugiere la frase "simulación de base de datos".
- Las reglas (totales y RUT) se prueban aisladas, sin HTTP ni archivo.
- Al estar en proyectos separados, es el compilador el que impide las dependencias prohibidas, no la disciplina del equipo.

**Alternativas evaluadas:**

| Alternativa | Por qué no se eligió |
|---|---|
| Vertical Slice (una carpeta por endpoint) | Encaja muy bien con una API de solo consulta y tiene menos ceremonia, pero se reconoce menos como respuesta al "patrón" que pide el enunciado |
| Capas como carpetas en un solo proyecto | La misma separación con menos proyectos, pero nada impide que una capa dependa de otra que no debe |
| Hexagonal (puertos y adaptadores) | En la práctica equivale a lo elegido; solo cambia el vocabulario |
| CQRS con MediatR | Agrega indirección (requests, handlers, pipeline) sin beneficio para 6 consultas |

**Se dejó fuera deliberadamente** para que la arquitectura sea proporcional al problema:
- Sin MediatR ni CQRS: los casos de uso son métodos de `FacturaService`.
- Sin AutoMapper: el mapeo a DTO se hace a mano y se lee en un solo archivo (`FacturaMappings`).
- Sin repositorio genérico ni Unit of Work: el puerto tiene un único método, `GetAllAsync`.
- Sin un proyecto de contratos compartidos ni capas adicionales.

**Consecuencias.**
- Se aceptan costos fijos: un archivo `DependencyInjection` por capa, interfaces con una sola implementación y tres modelos para factura, detalle y producto (justificados en [D-08](#d-08--record-para-datos-inmutables-class-para-la-entidad)).
- La interfaz del repositorio es `async` aunque hoy los datos están en memoria, para no tener que cambiar la firma si se conecta una base de datos.
- Si el alcance creciera (escrituras, más reglas, más entidades), esta estructura se extiende sin reorganizar el código. Si se quedara en solo lectura, Vertical Slice sería una simplificación válida.

### D-02 — Totales como propiedades calculadas del dominio
`DetalleFactura.Total` y `Factura.Total` son propiedades derivadas, así que no se pueden desincronizar. Los totales del JSON no se mapean al dominio (RNF-01, constitución II).

### D-03 — ApiKey + JWT (reemplazada por D-07)
> **Reemplazada.** Se conserva como registro histórico. La ApiKey sigue vigente tal como se describe aquí; la parte de JWT fue removida en [D-07](#d-07--remoción-de-jwt).

- El enunciado exige ApiKey y la indicación inicial del ADR pedía además JWT, así que se aplicaron ambos:
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

**Contexto.** El enunciado pide proteger todos los métodos con ApiKey. El JWT se había agregado porque la versión inicial de la indicación del ADR lo pedía; esa parte se retiró de la indicación (ver [enunciado.md](enunciado.md#indicación-adicional-del-adr)), así que el JWT dejó de tener un requisito que lo respalde. Además, mantener ambos obligaba al consumidor a dos pasos (obtener token y luego llamar) y sumaba configuración (clave de firma, clientes, scopes) que el enunciado no pide evaluar.

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

### D-09 — Manejo de errores sin `try/catch` y validación al arranque
**Fecha:** 2026-10-27 · **Estado:** Aceptada

**Contexto.** El enunciado pide "códigos HTTP adecuados según el tipo de respuesta". Al revisar el código surgió la pregunta de por qué no hay `try/catch`, y una prueba con un archivo de datos inexistente mostró que la API arrancaba igual y respondía 500 en cada petición.

**Decisión.** Los errores se clasifican en tres niveles y cada uno se maneja en un único lugar:

1. **Errores esperados (entrada del cliente):** se resuelven con condiciones, no con excepciones.

   | Caso | Mecanismo | Respuesta |
   |---|---|---|
   | RUT mal formado o con DV incorrecto | `Rut.TryParse` devuelve `false` | 400 |
   | Comuna ≤ 0 | `if (comuna <= 0)` en el endpoint | 400 |
   | Comuna no numérica (`/comunas/abc`) | Restricción de ruta `{comuna:int}` | 404 |
   | Búsqueda sin resultados | `facturas.Count == 0` | 404 |
   | ApiKey ausente o inválida | `ApiKeyEndpointFilter` | 401 |

2. **Errores de configuración y datos:** se detectan **al arrancar** y la aplicación no inicia.
   - ApiKey sin configurar → `ValidateOnStart` en `SecurityExtensions`.
   - JSON inexistente o inválido → `Program.cs` resuelve `IFacturaRepository` antes de `app.Run()`, y eso fuerza la carga del archivo.

3. **Errores inesperados en ejecución (bugs, fallos de I/O):** los atrapa un único manejador global, `AddProblemDetails()` + `UseExceptionHandler()` en `Program.cs`.
   - **El cliente recibe** un 500 en formato `application/problem+json` con un `traceId`, sin stack trace ni rutas internas.
   - **El log registra** la excepción completa (`ExceptionHandlerMiddleware`) con el mismo `traceId`, para poder relacionar un reclamo con su causa.

**Por qué no hay `try/catch`.** En ningún punto del código existe un error del que un `catch` pueda recuperarse: lo único que podría hacer es registrar el error y devolver 500, que es exactamente lo que ya hace el manejador global. Repetirlo en cada endpoint o servicio arriesgaría respuestas inconsistentes o errores silenciados. Si en el futuro aparece un error recuperable (por ejemplo, reintentar una conexión a base de datos), ese caso puntual sí llevaría su `try/catch`.

**Consecuencias.**
- Una aplicación que logra arrancar tiene configuración y datos válidos.
- Toda respuesta de error, esperada o no, tiene el mismo formato.
- Verificado por `ArranqueTests.Archivo_de_datos_inexistente_impide_el_arranque` y por las pruebas de códigos HTTP de `FacturasApiTests`.

### D-10 — Variante Vertical Slice (rama `vertical-slice`)
**Fecha:** 2026-10-27 · **Estado:** Propuesta, para comparar con D-01

**Contexto.** D-01 menciona Vertical Slice como alternativa válida si la API se mantiene de solo lectura. Se implementó en una rama aparte para comparar con código real y no solo en teoría.

**Decisión.** Un único proyecto `Facturas.Api`, con un archivo por requisito en `Features/` (endpoint + lógica + respuesta) y lo compartido en `Common/`. La sección "Estructura" de este plan describe la versión de `main`; la de esta rama está en el README.

| Elemento en `main` | En `vertical-slice` |
|---|---|
| Proyectos `Domain`, `Application`, `Infrastructure` | Carpetas `Common/Domain` y `Common/Data` dentro de `Facturas.Api` |
| `IFacturaService` / `FacturaService` | La lógica de cada consulta vive en su slice |
| `IFacturaRepository` / `JsonFacturaRepository` | `FacturaStore` (sin interfaz, sin `async`) |
| `CompradorTotalDto`, `ComunaFacturasDto` | Un `record` de respuesta dentro de cada slice |
| `FacturaDto` + `FacturaMappings` | `Common/Contracts/FacturaDto` con `FacturaDto.From` |
| `FacturaEndpoints`, `CompradorEndpoints` | Un `Map` por slice, registrado en `Program.cs` |
| `FacturaServiceTests` | `FeaturesTests`: cada slice se prueba a través de su endpoint |

**Decisiones internas de la variante:**
- `CompradorMayorCompra` reutiliza `CompradoresConTotal.Calcular` en vez de repetir la agrupación. Es el único acoplamiento entre slices, y queda dentro de la misma carpeta.
- `FacturaStore` es síncrono y no tiene interfaz: con los datos en memoria, el `async` solo agregaba ruido. Si se conectara una base de datos, habría que reintroducir una interfaz y tocar los 6 slices.
- Se conservan sin cambios el dominio, la validación al arranque (D-09), la ApiKey y los ProblemDetails.

**Verificación.** Build sin advertencias y 31/31 pruebas. Las respuestas HTTP de los 11 casos de prueba (cuerpo y código) y las rutas de OpenAPI son **idénticas** a las de `main`.

**Consecuencias.**
- Se pasa de 5 proyectos a 2 (API y pruebas) y desaparecen 2 interfaces y 2 archivos `DependencyInjection`.
- La separación entre dominio y endpoints depende de las carpetas, no del compilador.
- Cambiar la fuente de datos cuesta más que en `main`.

## Cumplimiento de la constitución

| Principio | Cómo se cumple |
|---|---|
| I | `JsonFacturaRepository` es la única implementación de `IFacturaRepository` |
| II | D-02 |
| III | ApiKey de D-03, D-05, D-07 |
| IV | `TypedResults.Problem`, ProblemDetails global, D-09 |
| V | D-01 |
| VI | Ver la trazabilidad en [tasks.md](tasks.md) |
