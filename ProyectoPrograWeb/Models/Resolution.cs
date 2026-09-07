using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

/// <summary>
/// Detalle de como se resolvio un corte. Es inmutable: una vez escrita no se
/// puede modificar ni volver a escribir. El id del documento es el id del reporte,
/// de modo que un reporte no puede tener dos resoluciones.
/// </summary>
[FirestoreData]
public class Resolution
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string ReportId { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    [FirestoreProperty] public string TechnicianId { get; set; } = string.Empty;

    [FirestoreProperty] public string TechnicianName { get; set; } = string.Empty;

    /// <summary>Causa del corte, por ejemplo "transformador quemado".</summary>
    [FirestoreProperty] public string Cause { get; set; } = string.Empty;

    /// <summary>Detalle en texto estructurado de lo que se hizo.</summary>
    [FirestoreProperty] public string Detail { get; set; } = string.Empty;

    /// <summary>Tiempo estimado de resolucion que dio el tecnico, en minutos.</summary>
    [FirestoreProperty] public int EstimatedMinutes { get; set; }

    /// <summary>Hora exacta en la que se restablecio el servicio.</summary>
    [FirestoreProperty] public DateTime RestoredAt { get; set; }

    /// <summary>Minutos reales entre el inicio del corte y el restablecimiento.</summary>
    [FirestoreProperty] public double ResolutionMinutes { get; set; }

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
