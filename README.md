# Facturas API

Minimal API en **.NET 10** que expone consultas sobre facturas usando `Ejemplo.json` como única fuente de datos.
El total de cada factura (y de cada línea) se calcula en ejecución a partir de `CantidadProducto × Producto.Valor`; los campos `TotalFactura`/`TotalProducto` del JSON se ignoran.

## Arquitectura (Clean Architecture)

```
src/
  Facturas.Domain          Entidades (Factura, DetalleFactura, Producto) y value object Rut (validación módulo 11). Sin dependencias.
  Facturas.Application     Casos de uso (IFacturaService/FacturaService), DTOs y el puerto IFacturaRepository.
  Facturas.Infrastructure  JsonFacturaRepository: lee el JSON una vez, lo mapea al dominio y lo mantiene en memoria.
  Facturas.Api             Minimal API: endpoints, ApiKey, JWT, ProblemDetails, OpenAPI + Swagger UI.
tests/
  Facturas.Tests           Pruebas de dominio, de aplicación (sobre el JSON real) e integración HTTP.
```

Las dependencias apuntan hacia adentro: `Api → Application/Infrastructure → Domain`; `Infrastructure` implementa el puerto definido en `Application`.

## Seguridad

Doble control en todos los endpoints de `/api/v1`:

1. **ApiKey** en el header `X-Api-Key` (endpoint filter aplicado al grupo raíz, incluido el endpoint de token).
2. **JWT Bearer** (HS256) con el claim `scope=facturas:read` para los endpoints de negocio.

Flujo: `POST /api/v1/auth/token` con ApiKey + `clientId`/`clientSecret` → se obtiene el JWT → se llama a los endpoints con ApiKey + `Authorization: Bearer <jwt>`.

Los valores de desarrollo están en `appsettings.Development.json`. En otros entornos deben definirse por variables de entorno o un gestor de secretos (`ApiKey__Key`, `Jwt__SigningKey`, `ApiClients__Clients__0__ClientId`, ...); la aplicación no arranca si faltan.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/v1/auth/token` | Emite un JWT (solo ApiKey) |
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
| 400 | RUT con formato/DV inválido, comuna ≤ 0, credenciales faltantes en `/auth/token` |
| 401 | ApiKey ausente o inválida, JWT ausente/inválido/expirado, credenciales de cliente inválidas |
| 403 | JWT válido sin el scope `facturas:read` |
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

Credenciales de desarrollo: ApiKey `dev-api-key-7f3c2a9e4b1d`, cliente `facturas-client` / `dev-client-secret`.

## Pruebas

```bash
dotnet test
```
