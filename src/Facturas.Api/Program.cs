using Facturas.Api.Common.Data;
using Facturas.Api.Common.OpenApi;
using Facturas.Api.Common.Security;
using Facturas.Api.Features.Compradores;
using Facturas.Api.Features.Facturas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FacturaDataOptions>(builder.Configuration.GetSection(FacturaDataOptions.SectionName));
builder.Services.AddSingleton<FacturaStore>();
builder.Services.AddApiSecurity(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<SecuritySchemesTransformer>());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Facturas API v1"));
}

app.UseHttpsRedirection();

// Todos los endpoints bajo /api/v1 exigen ApiKey.
var api = app.MapGroup("/api/v1")
    .AddEndpointFilter<ApiKeyEndpointFilter>()
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

// Un slice por requisito del enunciado.
ListarFacturas.Map(api);              // RF-01
FacturasPorComprador.Map(api);        // RF-02
CompradorMayorCompra.Map(api);        // RF-03
CompradoresConTotal.Map(api);         // RF-04
FacturasAgrupadasPorComuna.Map(api);  // RF-05
FacturasPorComuna.Map(api);           // RF-06

// Carga el JSON al arrancar: si falta o es inválido, la aplicación no inicia (en vez de responder 500 en cada petición).
app.Services.GetRequiredService<FacturaStore>();

app.Run();

public partial class Program;
