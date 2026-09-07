using System.Security.Claims;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// UID de Firebase del usuario autenticado. Los tokens de Firebase traen el uid
    /// tanto en "user_id" como en "sub"; se revisan los dos y tambien el nombre
    /// mapeado por si alguien vuelve a activar MapInboundClaims.
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst("user_id")?.Value
        ?? user.FindFirst("sub")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>Igual que GetUserId pero lanza 403 si no hay sesion, para usar en los controladores.</summary>
    public static string RequireUserId(this ClaimsPrincipal user) =>
        user.GetUserId() ?? throw new ForbiddenException("No se pudo identificar al usuario del token", "sin_sesion");

    /// <summary>Rol que viene en el custom claim de Firebase.</summary>
    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirst("role")?.Value
        ?? user.FindFirst(ClaimTypes.Role)?.Value
        ?? Roles.Ciudadano;

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value ?? string.Empty;

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.GetRole() == Roles.Administrador;
}
