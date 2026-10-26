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
