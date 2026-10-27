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
| `Rut` (value object) | Numero, Dv | Número entre 1 y 999.999.999; DV normalizado a mayúscula y validado por módulo 11 tanto en `Rut.TryParse` (request) como en `Rut.Create` (JSON) |

`Factura` es una entidad (`class`, identificada por `NumeroDocumento`). `DetalleFactura`, `Producto` y `Rut` son objetos de valor (`record`, igualdad por valor). Ver [D-08](plan.md#d-08--record-para-datos-inmutables-class-para-la-entidad).

## Lectura (Infrastructure)

`FacturaRecord`, `DetalleFacturaRecord` y `ProductoRecord` reflejan el JSON con los números como `decimal`, porque System.Text.Json no convierte `1569.0` a `long`. `TotalFactura` y `TotalProducto` **no se declaran**, así que es imposible leerlos por error.

Las validaciones de la fuente se ejecutan antes del mapeo al dominio; las entidades `Factura`, `DetalleFactura` y `Producto` no implementan por sí mismas todas estas comprobaciones.

### Validaciones al cargar el JSON

Todos los parámetros de los modelos de lectura son obligatorios (`RespectRequiredConstructorParameters = true`). Los campos de totales no pertenecen a esos modelos: pueden omitirse y, si están presentes, se ignoran.

| Dato | Validación |
|---|---|
| Raíz | Arreglo de facturas. `[]` es válido; archivo vacío, JSON mal formado o raíz `null` impiden el arranque |
| Factura | No puede ser `null` |
| `NumeroDocumento` | Entero positivo representable como `long` |
| RUT de comprador y vendedor | Número entero entre 1 y 999.999.999 y DV obligatorio de un carácter, válido por módulo 11; acepta `k` o `K` |
| `DireccionComprador` | No nula, vacía ni compuesta solo por espacios |
| `ComunaComprador`, `RegionComprador` | Enteros positivos representables como `int` |
| `DetalleFactura` | Colección no nula con al menos una línea; ninguna línea puede ser `null` |
| `CantidadProducto` | Entero positivo representable como `int` |
| `Producto` | No puede ser `null` |
| `Descripcion` | No nula, vacía ni compuesta solo por espacios |
| `Valor` | Número `decimal` no negativo; admite cero y fracciones |

Los números con notación decimal pero sin fracción, como `1569.0`, son válidos para campos enteros. Los valores fraccionarios no se redondean ni se truncan silenciosamente: se rechazan. No se comprueba la pertenencia de comuna y región a un catálogo ni su relación entre sí.

`Rut.Create` lanza una excepción ante un número o DV inválido; la carga no continúa y la aplicación no inicia. `Rut.TryParse` devuelve `false` ante una entrada inválida del request, que el endpoint traduce a HTTP 400. El request puede omitir el DV y calcularlo; en el JSON ambos campos son obligatorios. Ninguna de estas comprobaciones modifica el archivo original.

## Salida (DTOs)

| DTO | Uso | Campos |
|---|---|---|
| `FacturaDto` | RF-01, 02, 06 | Misma forma que el JSON, con `TotalFactura` y `TotalProducto` calculados |
| `CompradorTotalDto` | RF-03, 04 | RUTComprador, DvComprador, CantidadFacturas, MontoTotalCompras |
| `ComunaFacturasDto` | RF-05 | Comuna, CantidadFacturas, MontoTotal, Facturas |
