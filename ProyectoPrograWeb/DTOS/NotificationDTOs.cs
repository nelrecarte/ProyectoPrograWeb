using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.DTOs;

public class NotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public static NotificationDto From(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        ReportId = n.ReportId,
        ZoneName = n.ZoneName,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
