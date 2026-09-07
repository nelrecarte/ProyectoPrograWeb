using System.Text.Json;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Middlewares;

/// <summary>
/// Convierte cualquier excepcion en una respuesta JSON con la misma forma, para que
/// el frontend nunca tenga que interpretar un stack trace ni adivinar el codigo.
///
/// Forma de la respuesta:
///   { "error": "mensaje para mostrar", "code": "codigo_estable", "data": { ... } }
/// </summary>
public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            // Errores de negocio esperados: no son fallas, no van al log como error.
            _logger.LogInformation("Regla de negocio: {Code} - {Message}", ex.Code, ex.Message);
            await WriteAsync(context, ex.StatusCode, ex.Message, ex.Code, ex.Data2);
        }
        catch (FirebaseAuthException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, ex.Message, "error_de_firebase");
        }
        catch (KeyNotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, ex.Message, "no_encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);

            await WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                _environment.IsDevelopment() ? ex.Message : "Ocurrio un error inesperado en el servidor",
                "error_interno");
        }
    }

    private static async Task WriteAsync(
        HttpContext context, int statusCode, string message, string code, object? data = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message, code, data }, JsonOptions));
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ErrorHandlingMiddleware>();
}
