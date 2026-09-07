using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

/// <summary>
/// Confirmacion comunitaria: un vecino dice que el corte tambien le afecta.
/// El id del documento es "{reportId}_{userId}", asi Firestore mismo impide
/// que una persona confirme dos veces el mismo reporte.
/// </summary>
[FirestoreData]
public class Confirmation
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string ReportId { get; set; } = string.Empty;

    [FirestoreProperty] public string UserId { get; set; } = string.Empty;

    [FirestoreProperty] public string UserName { get; set; } = string.Empty;

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static string BuildId(string reportId, string userId) => $"{reportId}_{userId}";
}
