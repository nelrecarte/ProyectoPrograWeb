using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

/// <summary>
/// Estados por los que pasa un reporte de corte. Se guardan como texto en Firestore
/// para que el documento se pueda leer directo desde la consola.
/// </summary>
public static class ReportStatus
{
    public const string Nuevo = "nuevo";
    public const string EnVerificacion = "en_verificacion";
    public const string Confirmado = "confirmado";
    public const string Resuelto = "resuelto";

    public static readonly string[] Todos =
        [Nuevo, EnVerificacion, Confirmado, Resuelto];

    /// <summary>Estados en los que el corte sigue abierto.</summary>
    public static readonly string[] Abiertos =
        [Nuevo, EnVerificacion, Confirmado];

    public static bool EsValido(string status) => Todos.Contains(status);
}

/// <summary>
/// Reporte de un corte de energia en una zona.
/// </summary>
[FirestoreData]
public class OutageReport
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    /// <summary>Nombre de la zona copiado al crear el reporte, para no tener que leer la zona en cada listado.</summary>
    [FirestoreProperty] public string ZoneName { get; set; } = string.Empty;

    /// <summary>Direccion aproximada que escribe el ciudadano.</summary>
    [FirestoreProperty] public string Address { get; set; } = string.Empty;

    /// <summary>Hora a la que, segun el ciudadano, empezo el corte.</summary>
    [FirestoreProperty] public DateTime StartedAt { get; set; }

    [FirestoreProperty] public string Status { get; set; } = ReportStatus.Nuevo;

    /// <summary>
    /// Bandera derivada del estado: true mientras el corte no este resuelto.
    /// Existe para poder buscar el reporte activo de una zona con dos filtros de igualdad,
    /// que es lo que permite el guard de duplicados sin indices compuestos.
    /// </summary>
    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public string ReportedByUserId { get; set; } = string.Empty;

    [FirestoreProperty] public string ReportedByName { get; set; } = string.Empty;

    [FirestoreProperty] public string AssignedTechnicianId { get; set; } = string.Empty;

    [FirestoreProperty] public string AssignedTechnicianName { get; set; } = string.Empty;

    /// <summary>URL de la evidencia fotografica. Queda como campo opcional; la subida a Storage esta fuera de alcance.</summary>
    [FirestoreProperty] public string EvidenceUrl { get; set; } = string.Empty;

    /// <summary>Cuantos vecinos confirmaron que el corte tambien les afecta.</summary>
    [FirestoreProperty] public int ConfirmationCount { get; set; }

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Hora en la que el tecnico registro el restablecimiento del servicio.</summary>
    [FirestoreProperty] public DateTime? ResolvedAt { get; set; }
}
