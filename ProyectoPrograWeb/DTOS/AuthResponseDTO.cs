using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class AuthResponseDto
{
    /// <summary>Token de Firebase. Va en el header Authorization: Bearer {idToken}.</summary>
    public string IdToken { get; set; } = string.Empty;

    /// <summary>UID de Firebase del usuario.</summary>
    public string LocalId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Rol del usuario, para que el frontend sepa a que panel mandarlo.</summary>
    public string Role { get; set; } = Roles.Ciudadano;
}
