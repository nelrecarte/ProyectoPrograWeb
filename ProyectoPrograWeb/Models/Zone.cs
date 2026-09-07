using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

/// <summary>
/// Zona o sector de cobertura. Es el catalogo contra el que se reportan los cortes.
/// </summary>
[FirestoreData]
public class Zone
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    /// <summary>Nombre visible de la zona, por ejemplo "Colonia Kennedy".</summary>
    [FirestoreProperty] public string Name { get; set; } = string.Empty;

    /// <summary>Sector o municipio al que pertenece la zona.</summary>
    [FirestoreProperty] public string Sector { get; set; } = string.Empty;

    [FirestoreProperty] public string Description { get; set; } = string.Empty;

    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
