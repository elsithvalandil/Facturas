using Facturas.Api.Endpoints;
using Facturas.Api.OpenApi;
using Facturas.Api.Security;
using Facturas.Application;
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
app.UseAuthentication();
app.UseAuthorization();

// Todos los endpoints bajo /api/v1 exigen ApiKey; los de negocio además exigen JWT con scope facturas:read.
var api = app.MapGroup("/api/v1")
    .AddEndpointFilter<ApiKeyEndpointFilter>()
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

api.MapAuthEndpoints();
api.MapFacturaEndpoints();
api.MapCompradorEndpoints();

app.Run();

public partial class Program;
