using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

public static class ReportStatus
{
    public const string Nuevo = "nuevo";
    public const string EnVerificacion = "en_verificacion";
    public const string Confirmado = "confirmado";
    public const string Resuelto = "resuelto";

    public static readonly string[] Todos =
        [Nuevo, EnVerificacion, Confirmado, Resuelto];

    public static readonly string[] Abiertos =
        [Nuevo, EnVerificacion, Confirmado];

    public static bool EsValido(string status) => Todos.Contains(status);
}

[FirestoreData]
public class OutageReport
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneName { get; set; } = string.Empty;

    [FirestoreProperty] public string Address { get; set; } = string.Empty;

    [FirestoreProperty] public DateTime StartedAt { get; set; }

    [FirestoreProperty] public string Status { get; set; } = ReportStatus.Nuevo;

    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public string ReportedByUserId { get; set; } = string.Empty;

    [FirestoreProperty] public string ReportedByName { get; set; } = string.Empty;

    [FirestoreProperty] public string AssignedTechnicianId { get; set; } = string.Empty;

    [FirestoreProperty] public string AssignedTechnicianName { get; set; } = string.Empty;

    [FirestoreProperty] public string EvidenceUrl { get; set; } = string.Empty;

    [FirestoreProperty] public int ConfirmationCount { get; set; }

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty] public DateTime? ResolvedAt { get; set; }
}
