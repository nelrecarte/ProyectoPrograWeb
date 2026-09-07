using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Models;

[FirestoreData]
public class AppUser
{
    [FirestoreProperty] public string Id { get; set; } = string.Empty;

    [FirestoreProperty] public string Email { get; set; } = string.Empty;

    [FirestoreProperty] public string DisplayName { get; set; } = string.Empty;

    [FirestoreProperty] public string Username { get; set; } = string.Empty;

    [FirestoreProperty] public string PhoneNumber { get; set; } = string.Empty;

    [FirestoreProperty] public DateTime BirthDate { get; set; }

    [FirestoreProperty] public string Country { get; set; } = string.Empty;

    [FirestoreProperty] public string Bio { get; set; } = string.Empty;

    [FirestoreProperty] public string Role { get; set; } = Roles.Ciudadano;

    /// <summary>Zona del ciudadano o zona de cobertura del tecnico. Vacio para el administrador.</summary>
    [FirestoreProperty] public string ZoneId { get; set; } = string.Empty;

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty] public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Los tres roles que pide el enunciado. Viajan al frontend dentro del ID token
/// como custom claim "role" de Firebase.
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Tecnico = "Tecnico";
    public const string Ciudadano = "Ciudadano";

    public static readonly string[] Todos = [Administrador, Tecnico, Ciudadano];

    public static bool EsValido(string role) => Todos.Contains(role);
}
