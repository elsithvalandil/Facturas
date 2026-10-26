# Constitución del proyecto Facturas

Principios no negociables. Toda spec, plan o cambio de código debe respetarlos; si alguno debe cambiar, se modifica este documento primero.

## I. Fuente de datos única e inmutable
`Ejemplo.json` es la única fuente de datos. Su estructura no se modifica y no se agregan otras fuentes (BD, caché externa, archivos auxiliares).

## II. Totales calculados en ejecución
El total de cada línea es `CantidadProducto × Producto.Valor` y el de la factura es la suma de sus líneas. Los campos `TotalFactura` / `TotalProducto` del JSON se ignoran siempre.

## III. Seguridad en todos los endpoints
- Toda ruta bajo `/api/v1` exige ApiKey válida (header `X-Api-Key`).
- Ningún secreto real se versiona; la app no arranca si falta.

## IV. Semántica HTTP correcta
200 éxito · 400 entrada inválida · 401 ApiKey ausente o inválida · 404 sin resultados · 500 error no controlado. Todo error se responde como `application/problem+json`.

## V. Clean Architecture
Dependencias hacia adentro: `Api → Application/Infrastructure → Domain`. El dominio no depende de nada; la infraestructura implementa puertos definidos en Application.

## VI. Verificable por pruebas
Cada requisito funcional tiene al menos una prueba automatizada que lo cubre. `dotnet build` y `dotnet test` deben pasar.

## Stack fijado (ADR)
.NET 10 · Minimal API · OpenAPI + Swagger UI · xUnit.

## Historial de enmiendas
| Fecha | Cambio |
|---|---|
| 2026-10-26 | Se elimina JWT del principio III y del stack; se quita el 403 del principio IV. Motivo: [D-07](../../specs/001-api-consulta-facturas/plan.md#d-07--remoción-de-jwt). |
