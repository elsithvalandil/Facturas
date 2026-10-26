# Modelo de datos — Spec 001

## Fuente (JSON)

Archivo: [`src/Facturas.Api/Data/Ejemplo.json`](../../src/Facturas.Api/Data/Ejemplo.json). Arreglo de facturas; los números vienen como `double` (`1569.0`).

```
Factura
├─ NumeroDocumento      number
├─ RUTVendedor          number
├─ DvVendedor           string  ("0"-"9" | "k"/"K")
├─ RUTComprador         number
├─ DvComprador          string
├─ DireccionComprador   string
├─ ComunaComprador      number
├─ RegionComprador      number
├─ TotalFactura         number  ← ignorado
└─ DetalleFactura[]
   ├─ CantidadProducto  number
   ├─ Producto { Descripcion: string, Valor: number }
   └─ TotalProducto     number  ← ignorado
```

## Dominio

| Tipo | Campos | Reglas |
|---|---|---|
| `Factura` | NumeroDocumento, Vendedor (Rut), Comprador (Rut), DireccionComprador, ComunaComprador, RegionComprador, Detalle | `Total = Σ Detalle.Total` |
| `DetalleFactura` | Cantidad, Producto | `Total = Cantidad × Producto.Valor` |
| `Producto` | Descripcion, Valor | — |
| `Rut` (value object) | Numero, Dv | Se normaliza el DV a mayúscula. El DV se valida por módulo 11 solo cuando el RUT llega como entrada de la API (`Rut.TryParse`); los RUT del JSON se cargan tal como vienen (`Rut.Create`) |

`Factura` es una entidad (`class`, identificada por `NumeroDocumento`). `DetalleFactura`, `Producto` y `Rut` son objetos de valor (`record`, igualdad por valor). Ver [D-08](plan.md#d-08--record-para-datos-inmutables-class-para-la-entidad).

## Lectura (Infrastructure)

`FacturaRecord`, `DetalleFacturaRecord` y `ProductoRecord` reflejan el JSON con los números como `decimal`, porque System.Text.Json no convierte `1569.0` a `long`. `TotalFactura` y `TotalProducto` **no se declaran**, así que es imposible leerlos por error.

## Salida (DTOs)

| DTO | Uso | Campos |
|---|---|---|
| `FacturaDto` | RF-01, 02, 06 | Misma forma que el JSON, con `TotalFactura` y `TotalProducto` calculados |
| `CompradorTotalDto` | RF-03, 04 | RUTComprador, DvComprador, CantidadFacturas, MontoTotalCompras |
| `ComunaFacturasDto` | RF-05 | Comuna, CantidadFacturas, MontoTotal, Facturas |
