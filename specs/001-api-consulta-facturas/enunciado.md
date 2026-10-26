# Enunciado original

> Texto del ejercicio tal como fue entregado. No editar: es la fuente de la que se derivan [spec.md](spec.md) y los demás artefactos.
> Fecha de recepción: 2026-10-26.

Dado el siguiente JSON con una lista de facturas, deberá utilizarlo como base de datos (no contiene el total calculado de cada factura).

El artefacto desarrollado deberá exponer distintos métodos que permitan realizar las siguientes acciones:

- Retornar la lista completa de facturas, calculando dinámicamente el total de cada una en base al detalle de productos.
- Retornar las facturas de un comprador según su RUT.
- Retornar el comprador que tenga el mayor monto total acumulado en compras.
- Retornar la lista de compradores con el monto total de compras realizadas.
- Retornar la lista de facturas agrupadas por comuna.
- Retornar las facturas correspondientes a una comuna específica.

## Requisitos Técnicos

- Debe desarrollarse en .NET Core 8 o superior.
- El JSON debe utilizarse como única fuente de datos (simulación de base de datos).
- El total de cada factura debe calcularse en ejecución.
- Todos los métodos deben estar protegidos mediante una ApiKey.
- Debe utilizar códigos HTTP adecuados según el tipo de respuesta.
- Debe implementarse algún patrón de desarrollo (Ej. Clean Architecture).
- El proyecto debe compilar y ejecutarse correctamente.

## Fuente de Datos

Se debe utilizar exactamente el siguiente JSON sin modificar su estructura.

→ Ver [`src/Facturas.Api/Data/Ejemplo.json`](../../src/Facturas.Api/Data/Ejemplo.json).

## Indicación adicional del ADR

Como ADR, quiero que la solución que se cree esté en NET 10, implementando Minimal Api.