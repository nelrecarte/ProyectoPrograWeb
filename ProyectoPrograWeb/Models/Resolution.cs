using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

[FirestoreData]
public class Resolution
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string ReportId { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    [FirestoreProperty] public string TechnicianId { get; set; } = string.Empty;

    [FirestoreProperty] public string TechnicianName { get; set; } = string.Empty;

    [FirestoreProperty] public string Cause { get; set; } = string.Empty;

    [FirestoreProperty] public string Detail { get; set; } = string.Empty;

    [FirestoreProperty] public int EstimatedMinutes { get; set; }

    [FirestoreProperty] public DateTime RestoredAt { get; set; }

    [FirestoreProperty] public double ResolutionMinutes { get; set; }

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
