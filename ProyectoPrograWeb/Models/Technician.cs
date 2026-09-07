using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

/// <summary>
/// Tecnico de zona. El administrador lo registra y lo asocia a un usuario existente.
/// </summary>
[FirestoreData]
public class Technician
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    /// <summary>UID de Firebase del usuario que actua como tecnico.</summary>
    [FirestoreProperty] public string UserId { get; set; } = string.Empty;

    [FirestoreProperty] public string FullName { get; set; } = string.Empty;

    [FirestoreProperty] public string Email { get; set; } = string.Empty;

    /// <summary>Zona que cubre este tecnico.</summary>
    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneName { get; set; } = string.Empty;

    [FirestoreProperty] public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Baja logica. Un tecnico desactivado no recibe reportes nuevos
    /// pero conserva todo su historial.
    /// </summary>
    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
