using System.ComponentModel.DataAnnotations;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class CreateResolutionDto
{
    [Required, MinLength(3)] public string Cause { get; set; } = string.Empty;

    [Required, MinLength(10, ErrorMessage = "El detalle de la resolucion es muy corto")]
    public string Detail { get; set; } = string.Empty;

    [Range(0, 10080, ErrorMessage = "El tiempo estimado debe estar entre 0 y 10080 minutos")]
    public int EstimatedMinutes { get; set; }

    /// <summary>Hora exacta del restablecimiento. Si no se manda, se toma la hora actual.</summary>
    public DateTime? RestoredAt { get; set; }
}

public class ResolutionDto
{
    public string Id { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public DateTime RestoredAt { get; set; }
    public double ResolutionMinutes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ResolutionDto From(Resolution r) => new()
    {
        Id = r.Id,
        ReportId = r.ReportId,
        TechnicianId = r.TechnicianId,
        TechnicianName = r.TechnicianName,
        Cause = r.Cause,
        Detail = r.Detail,
        EstimatedMinutes = r.EstimatedMinutes,
        RestoredAt = r.RestoredAt,
        ResolutionMinutes = r.ResolutionMinutes,
        CreatedAt = r.CreatedAt
    };
}
