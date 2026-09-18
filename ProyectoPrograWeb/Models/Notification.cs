using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

[FirestoreData]
public class Notification
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string UserId { get; set; } = string.Empty;

    [FirestoreProperty] public string Type { get; set; } = string.Empty;

    [FirestoreProperty] public string Title { get; set; } = string.Empty;

    [FirestoreProperty] public string Message { get; set; } = string.Empty;

    [FirestoreProperty] public string ReportId { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneName { get; set; } = string.Empty;

    [FirestoreProperty] public bool IsRead { get; set; }

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class NotificationType
{
    public const string ReporteNuevo = "reporte_nuevo";
    public const string ReporteConfirmado = "reporte_confirmado";
    public const string ReporteResuelto = "reporte_resuelto";
}
