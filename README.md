# Facturas API

Minimal API en **.NET 10** que expone consultas sobre facturas usando `Ejemplo.json` como única fuente de datos.
El total de cada factura (y de cada línea) se calcula en ejecución a partir de `CantidadProducto × Producto.Valor`; los campos `TotalFactura`/`TotalProducto` del JSON se ignoran.

## Especificación

El enunciado, la spec, el plan y las tareas están en [`specs/001-api-consulta-facturas/`](specs/001-api-consulta-facturas/). Los principios del proyecto están en [`.specify/memory/constitution.md`](.specify/memory/constitution.md).

## Arquitectura (Clean Architecture)

```
src/
  Facturas.Domain          Entidades (Factura, DetalleFactura, Producto) y value object Rut (validación módulo 11). Sin dependencias.
  Facturas.Application     Casos de uso (IFacturaService/FacturaService), DTOs y el puerto IFacturaRepository.
  Facturas.Infrastructure  JsonFacturaRepository: lee el JSON una vez, lo mapea al dominio y lo mantiene en memoria.
  Facturas.Api             Minimal API: endpoints, ApiKey, ProblemDetails, OpenAPI + Swagger UI.
tests/
  Facturas.Tests           Pruebas de dominio, de aplicación (sobre el JSON real) e integración HTTP.
```

Las dependencias apuntan hacia adentro: `Api → Application/Infrastructure → Domain`; `Infrastructure` implementa el puerto definido en `Application`.

## Seguridad

Todos los endpoints de `/api/v1` exigen una **ApiKey** en el header `X-Api-Key`. Se valida con un endpoint filter aplicado al grupo raíz, tal como pide el enunciado.

> Una versión anterior además exigía JWT, por indicación del ADR. Se removió para ajustarse al enunciado: ver la decisión [D-07](specs/001-api-consulta-facturas/plan.md#d-07--remoción-de-jwt).

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
