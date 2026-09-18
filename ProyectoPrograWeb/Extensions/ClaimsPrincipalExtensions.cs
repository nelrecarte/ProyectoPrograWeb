using System.Security.Claims;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst("user_id")?.Value
        ?? user.FindFirst("sub")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string RequireUserId(this ClaimsPrincipal user) =>
        user.GetUserId() ?? throw new ForbiddenException("No se pudo identificar al usuario del token", "sin_sesion");

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirst("role")?.Value
        ?? user.FindFirst(ClaimTypes.Role)?.Value
        ?? Roles.Ciudadano;

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value ?? string.Empty;

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.GetRole() == Roles.Administrador;
}
