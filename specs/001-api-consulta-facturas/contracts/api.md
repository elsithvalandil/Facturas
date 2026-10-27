# Contrato HTTP — Spec 001

Base: `/api/v1`. El contrato ejecutable se publica en `/openapi/v1.json` (Development).

## Seguridad

Todas las rutas exigen el header `X-Api-Key: <apikey>`.

> El header `Authorization: Bearer` y el endpoint `POST /auth/token` se removieron el 2026-10-26 ([D-07](../plan.md#d-07--remoción-de-jwt)).

## Endpoints

| RF | Método | Ruta | 200 | Otros códigos |
|---|---|---|---|---|
| RF-01 | GET | `/facturas` | `FacturaDto[]` | — |
| RF-02 | GET | `/compradores/{rut}/facturas` | `FacturaDto[]` | 400 RUT inválido · 404 sin facturas |
| RF-03 | GET | `/compradores/mayor-compra` | `CompradorTotalDto` | 404 sin facturas |
| RF-04 | GET | `/compradores` | `CompradorTotalDto[]` (desc) | — |
| RF-05 | GET | `/facturas/comunas` | `ComunaFacturasDto[]` | — |
| RF-06 | GET | `/facturas/comunas/{comuna:int}` | `FacturaDto[]` | 400 comuna ≤ 0 · 404 sin facturas |

Comunes a todas las rutas: **401** si falta o es inválida la ApiKey · **500** si hay un error no controlado.

Todas las respuestas de error usan `application/problem+json`.

## Errores

### Entradas y fuente de datos

El RUT del request admite formatos con guion, puntos, DV `K` sin guion o solo número. El cuerpo admite de uno a nueve dígitos y debe representar un número positivo. Cuando se proporciona DV, debe coincidir con módulo 11; si se omite, se calcula. Un RUT inválido devuelve 400.

Los RUT del JSON se validan también por rango y módulo 11, pero el archivo debe incluir número y DV. Un error de validación del archivo impide el arranque y no genera una respuesta HTTP de consulta. Reglas completas: [modelo de datos](../data-model.md#validaciones-al-cargar-el-json).

Con una fuente `[]`, `/facturas`, `/compradores` y `/facturas/comunas` devuelven 200 con `[]`; las búsquedas por RUT o comuna y `/compradores/mayor-compra` devuelven 404. En un empate de compras, el ganador es el comprador con menor número de RUT.

### Formato de respuesta

Error esperado (por ejemplo, un RUT inválido): `title` y `detail` describen el problema.

```json
{ "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1", "title": "RUT inválido", "status": 400,
  "detail": "Formato esperado: 12345678-9, 12.345.678-K o solo el número. El DV debe ser válido.", "traceId": "00-…" }
```

Error inesperado (500): no expone detalles internos. El `traceId` permite ubicar la excepción en el log del servidor.

```json
{ "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1", "title": "An error occurred while processing your request.",
  "status": 500, "traceId": "00-…" }
```

Estrategia completa: [D-09](../plan.md#d-09--manejo-de-errores-sin-trycatch-y-validación-al-arranque).
