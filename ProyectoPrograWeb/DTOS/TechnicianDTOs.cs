using System.ComponentModel.DataAnnotations;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class CreateTechnicianDto
{
    /// <summary>UID de Firebase del usuario que va a ser tecnico. Debe estar ya registrado.</summary>
    [Required] public string UserId { get; set; } = string.Empty;

    [Required] public string FullName { get; set; } = string.Empty;

    [Required] public string ZoneId { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}

public class UpdateTechnicianDto
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public string ZoneId { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

public class TechnicianDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Cuantos reportes abiertos tiene asignados en este momento.</summary>
    public int ActiveReportCount { get; set; }

    public static TechnicianDto From(Technician t) => new()
    {
        Id = t.Id,
        UserId = t.UserId,
        FullName = t.FullName,
        Email = t.Email,
        ZoneId = t.ZoneId,
        ZoneName = t.ZoneName,
        IsAvailable = t.IsAvailable,
        IsActive = t.IsActive
    };
}
