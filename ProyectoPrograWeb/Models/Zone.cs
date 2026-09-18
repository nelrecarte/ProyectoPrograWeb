using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

[FirestoreData]
public class Zone
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string Name { get; set; } = string.Empty;

    [FirestoreProperty] public string Sector { get; set; } = string.Empty;

    [FirestoreProperty] public string Description { get; set; } = string.Empty;

    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
