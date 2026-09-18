using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

[FirestoreData]
public class Technician
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string UserId { get; set; } = string.Empty;

    [FirestoreProperty] public string FullName { get; set; } = string.Empty;

    [FirestoreProperty] public string Email { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneName { get; set; } = string.Empty;

    [FirestoreProperty] public bool IsAvailable { get; set; } = true;

    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
