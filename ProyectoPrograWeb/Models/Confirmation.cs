using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

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
