using System.ComponentModel.DataAnnotations;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class CreateZoneDto
{
    [Required, MinLength(3)] public string Name { get; set; } = string.Empty;
    [Required] public string Sector { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateZoneDto
{
    [Required, MinLength(3)] public string Name { get; set; } = string.Empty;
    [Required] public string Sector { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class ZoneDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>Reporte abierto de la zona, si lo hay. Sirve para que el formulario
    /// de reporte avise antes de intentar crear un duplicado.</summary>
    public string? ActiveReportId { get; set; }

    public static ZoneDto From(Zone zone) => new()
    {
        Id = zone.Id,
        Name = zone.Name,
        Sector = zone.Sector,
        Description = zone.Description,
        IsActive = zone.IsActive
    };
}
