using IntegradorMarcas.Api.Security;
using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Services;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var integraCnp = builder.Configuration.GetConnectionString("IntegraCnp");
    if (string.IsNullOrWhiteSpace(integraCnp))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:IntegraCnp no esta configurada para entorno no-Development.");
    }
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IJustificacionRepository, JustificacionRepository>();
builder.Services.AddScoped<IJustificacionService, JustificacionService>();
builder.Services.AddScoped<IUserContext, HeaderUserContext>();
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        var (statusCode, title) = exception switch
        {
            AppException appException => (appException.StatusCode, appException.Message),
            KeyNotFoundException keyNotFoundException => (StatusCodes.Status404NotFound, keyNotFoundException.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        context.Response.StatusCode = statusCode;
        await Results.Problem(title: title, statusCode: statusCode).ExecuteAsync(context);
    });
});
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var feature   = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;

        var correlationId = Guid.NewGuid();

        var (statusCode, title) = exception switch
        {
            AppException ex        => (ex.StatusCode, ex.Message),
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            OperationCanceledException => (499, "Solicitud cancelada por el cliente"),
            _                      => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        // Persistir en BD (fire-and-forget seguro, nunca lanza)
        try
        {
            var errorRepo = context.RequestServices.GetService<IErrorLogRepository>();
            if (errorRepo is not null && exception is not null)
            {
                var req     = context.Request;
                var userId  = context.Request.Headers.TryGetValue("X-User-Id",   out var uid)  ? uid.ToString()  : null;
                var userRole= context.Request.Headers.TryGetValue("X-User-Role", out var role) ? role.ToString() : null;
                var ip      = context.Connection.RemoteIpAddress?.ToString();
                var ua      = req.Headers.UserAgent.ToString();
                var env     = app.Environment.EnvironmentName;

                await errorRepo.LogAsync(new ErrorLogEntry(
                    CorrelationId: correlationId,
                    HttpMethod:    req.Method,
                    Endpoint:      req.Path.Value ?? "/",
                    StatusCode:    statusCode,
                    TipoError:     exception.GetType().Name,
                    Mensaje:       exception.Message,
                    StackTrace:    statusCode >= 500 ? exception.StackTrace : null,
                    UsuarioId:     userId,
                    RolUsuario:    userRole,
                    Entorno:       env,
                    Ip:            ip,
                    UserAgent:     ua
                ));
            }
        }
        catch { /* nunca propagar */ }

        context.Response.StatusCode = statusCode;
        context.Response.Headers["X-Correlation-Id"] = correlationId.ToString();

        await Results.Problem(
            title:      title,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["correlationId"] = correlationId }
        ).ExecuteAsync(context);
    });
});

var swaggerEnabled = builder.Configuration.GetValue("Swagger:Enabled", true);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("LocalFrontend");
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    utc = DateTime.UtcNow
}));

app.Run();
