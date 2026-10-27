using Facturas.Api.Endpoints;
using Facturas.Api.OpenApi;
using Facturas.Api.Security;
using Facturas.Application;
using Facturas.Application.Abstractions;
using Facturas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiSecurity(builder.Configuration);

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

api.MapFacturaEndpoints();
api.MapCompradorEndpoints();

// Carga el JSON al arrancar: si falta o es inválido, la aplicación no inicia (en vez de responder 500 en cada petición).
app.Services.GetRequiredService<IFacturaRepository>();

app.Run();

public partial class Program;
