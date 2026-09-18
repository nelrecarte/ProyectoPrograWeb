using System.ComponentModel.DataAnnotations;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class CreateReportDto
{
    [Required] public string ZoneId { get; set; } = string.Empty;

    [Required, MinLength(5, ErrorMessage = "La direccion aproximada es muy corta")]
    public string Address { get; set; } = string.Empty;

    public DateTime? StartedAt { get; set; }

    public string EvidenceUrl { get; set; } = string.Empty;

}

public class AssignTechnicianDto
{
    [Required] public string TechnicianId { get; set; } = string.Empty;
}

public class ChangeStatusDto
{
    [Required] public string Status { get; set; } = string.Empty;
}

public class ReportDto
{
    public string Id { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string ReportedByUserId { get; set; } = string.Empty;
    public string ReportedByName { get; set; } = string.Empty;
    public string AssignedTechnicianId { get; set; } = string.Empty;
    public string AssignedTechnicianName { get; set; } = string.Empty;
    public string EvidenceUrl { get; set; } = string.Empty;
    public int ConfirmationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public bool ConfirmedByMe { get; set; }

    public ResolutionDto? Resolution { get; set; }

    public static ReportDto From(OutageReport r) => new()
    {
        Id = r.Id,
        ZoneId = r.ZoneId,
        ZoneName = r.ZoneName,
        Address = r.Address,
        StartedAt = r.StartedAt,
        Status = r.Status,
        IsActive = r.IsActive,
        ReportedByUserId = r.ReportedByUserId,
        ReportedByName = r.ReportedByName,
        AssignedTechnicianId = r.AssignedTechnicianId,
        AssignedTechnicianName = r.AssignedTechnicianName,
        EvidenceUrl = r.EvidenceUrl,
        ConfirmationCount = r.ConfirmationCount,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        ResolvedAt = r.ResolvedAt
    };
}

public class DuplicateReportDto
{
    public string ExistingReportId { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ConfirmationCount { get; set; }
    public bool AlreadyConfirmedByMe { get; set; }
}
