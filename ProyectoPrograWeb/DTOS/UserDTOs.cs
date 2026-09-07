using System.ComponentModel.DataAnnotations;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static UserProfileDto From(AppUser u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        DisplayName = u.DisplayName,
        Username = u.Username,
        PhoneNumber = u.PhoneNumber,
        BirthDate = u.BirthDate,
        Country = u.Country,
        Bio = u.Bio,
        Role = u.Role,
        ZoneId = u.ZoneId,
        CreatedAt = u.CreatedAt
    };
}

public class UpdateProfileDto
{
    [Required] public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    /// <summary>Zona del ciudadano. Se usa para mostrarle los cortes de su sector.</summary>
    public string ZoneId { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    /// <summary>Administrador, Tecnico o Ciudadano.</summary>
    [Required] public string Role { get; set; } = string.Empty;
}
