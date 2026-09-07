namespace ProyectoQ3Backend.Exceptions;

/// <summary>
/// Base de los errores de negocio. El middleware de errores las traduce a
/// codigos HTTP para que el frontend no tenga que interpretar mensajes.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message, string code, object? data = null) : base(message)
    {
        Code = code;
        Data2 = data;
    }

    /// <summary>Codigo corto y estable, por ejemplo "reporte_duplicado".</summary>
    public string Code { get; }

    /// <summary>Datos extra que el frontend necesita para reaccionar.</summary>
    public object? Data2 { get; }

    public abstract int StatusCode { get; }
}

/// <summary>404 — el recurso no existe.</summary>
public class NotFoundException(string message, string code = "no_encontrado")
    : DomainException(message, code)
{
    public override int StatusCode => StatusCodes.Status404NotFound;
}

/// <summary>400 — la peticion no cumple una regla de negocio.</summary>
public class ValidationException(string message, string code = "peticion_invalida")
    : DomainException(message, code)
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
}

/// <summary>403 — el usuario esta autenticado pero no puede hacer esta accion.</summary>
public class ForbiddenException(string message, string code = "sin_permiso")
    : DomainException(message, code)
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
}

/// <summary>
/// 409 — choca con el estado actual del sistema. Lleva datos extra porque el caso
/// tipico es el reporte duplicado, donde el frontend necesita el id del reporte
/// que ya existe para ofrecer confirmarlo.
/// </summary>
public class ConflictException(string message, string code = "conflicto", object? data = null)
    : DomainException(message, code, data)
{
    public override int StatusCode => StatusCodes.Status409Conflict;
}
