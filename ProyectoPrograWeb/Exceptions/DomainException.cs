namespace ProyectoQ3Backend.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message, string code, object? data = null) : base(message)
    {
        Code = code;
        Data2 = data;
    }

    public string Code { get; }

    public object? Data2 { get; }

    public abstract int StatusCode { get; }
}

public class NotFoundException(string message, string code = "no_encontrado")
    : DomainException(message, code)
{
    public override int StatusCode => StatusCodes.Status404NotFound;
}

public class ValidationException(string message, string code = "peticion_invalida")
    : DomainException(message, code)
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
}

public class ForbiddenException(string message, string code = "sin_permiso")
    : DomainException(message, code)
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
}

public class ConflictException(string message, string code = "conflicto", object? data = null)
    : DomainException(message, code, data)
{
    public override int StatusCode => StatusCodes.Status409Conflict;
}
