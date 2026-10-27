# Facturas API

Minimal API en **.NET 10** que expone consultas sobre facturas usando `Ejemplo.json` como única fuente de datos.
El total de cada factura (y de cada línea) se calcula en ejecución a partir de `CantidadProducto × Producto.Valor`; los campos `TotalFactura`/`TotalProducto` del JSON se ignoran.

## Especificación

El enunciado, la spec, el plan y las tareas están en [`specs/001-api-consulta-facturas/`](specs/001-api-consulta-facturas/). Los principios del proyecto están en [`.specify/memory/constitution.md`](.specify/memory/constitution.md).

## Arquitectura (Vertical Slice)

> Esta es la rama `vertical-slice`, una variante de la solución de `main` (Clean Architecture) con **el mismo contrato HTTP**. Ver [D-10](specs/001-api-consulta-facturas/plan.md#d-10--variante-vertical-slice-rama-vertical-slice).

```
src/Facturas.Api/
  Program.cs               Registra los 6 slices, la ApiKey, ProblemDetails y OpenAPI + Swagger UI.
  Common/
    Domain/                Factura, DetalleFactura, Producto y Rut (validación módulo 11).
    Data/                  FacturaStore: lee el JSON una vez, lo mapea al dominio y lo mantiene en memoria.
    Contracts/             FacturaDto, compartido por 4 slices.
    Security/, OpenApi/    ApiKey y su documentación en OpenAPI.
  Features/
    Facturas/              ListarFacturas (RF-01), FacturasAgrupadasPorComuna (RF-05), FacturasPorComuna (RF-06).
    Compradores/           FacturasPorComprador (RF-02), CompradorMayorCompra (RF-03), CompradoresConTotal (RF-04).
tests/
  Facturas.Tests           Pruebas de dominio, de cada slice a través de su endpoint e integración HTTP.
```

Cada slice es un archivo con su endpoint, su lógica y su respuesta: un requisito del enunciado se revisa leyendo un solo archivo. Solo lo que usan varios slices vive en `Common/`.

## Seguridad

Todos los endpoints de `/api/v1` exigen una **ApiKey** en el header `X-Api-Key`. Se valida con un endpoint filter aplicado al grupo raíz, tal como pide el enunciado.

> Una versión anterior además exigía JWT, porque lo pedía la indicación inicial del ADR. Esa parte se retiró de la indicación y el JWT se removió para ajustarse al enunciado: ver la decisión [D-07](specs/001-api-consulta-facturas/plan.md#d-07--remoción-de-jwt).

El valor de desarrollo está en `appsettings.Development.json`. En otros entornos debe definirse por variable de entorno o gestor de secretos (`ApiKey__Key`); la aplicación no arranca si falta.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/v1/facturas` | Todas las facturas con total calculado |
| GET | `/api/v1/compradores/{rut}/facturas` | Facturas de un comprador (`21595854-k`, `21.595.854-K` o `21595854`) |
| GET | `/api/v1/compradores/mayor-compra` | Comprador con mayor monto acumulado |
| GET | `/api/v1/compradores` | Compradores con monto total (desc) |
| GET | `/api/v1/facturas/comunas` | Facturas agrupadas por comuna |
| GET | `/api/v1/facturas/comunas/{comuna}` | Facturas de una comuna |

### Códigos HTTP

| Código | Cuándo |
|---|---|
| 200 | Consulta exitosa |
| 400 | RUT con formato/DV inválido, comuna ≤ 0 |
| 401 | ApiKey ausente o inválida |
| 404 | Comprador o comuna sin facturas |
| 500 | Error no controlado (respuesta `ProblemDetails`) |

Todos los errores se devuelven como `application/problem+json`.

### Manejo de errores

No hay `try/catch` en el código, por diseño:

- **Entradas inválidas** (RUT, comuna, ApiKey): se validan con condiciones y devuelven 400, 401 o 404. No se usan excepciones como control de flujo.
- **Configuración o datos inválidos**: la aplicación **no arranca** si falta la ApiKey o si `Ejemplo.json` no existe o es inválido.
- **Errores inesperados**: los atrapa un único manejador global (`UseExceptionHandler`). El cliente recibe un 500 sin detalles internos y el log registra la excepción con el mismo `traceId`.

Detalle y justificación: [D-09](specs/001-api-consulta-facturas/plan.md#d-09--manejo-de-errores-sin-trycatch-y-validación-al-arranque).

## Ejecutar

```bash
dotnet run --project src/Facturas.Api --launch-profile http
```

- UI de documentación (Swagger): http://localhost:5116/swagger
- OpenAPI: http://localhost:5116/openapi/v1.json
- Peticiones de ejemplo: `src/Facturas.Api/Facturas.Api.http`

ApiKey de desarrollo: `dev-api-key-7f3c2a9e4b1d`.

## Pruebas

```bash
dotnet test
```
