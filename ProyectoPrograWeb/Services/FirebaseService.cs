using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Services;

/// <summary>Nombres de las colecciones de Firestore, en un solo lugar.</summary>
public static class Collections
{
    public const string Users = "users";
    public const string Zones = "zones";
    public const string Reports = "reports";
    public const string Confirmations = "confirmations";
    public const string Resolutions = "resolutions";
    public const string Technicians = "technicians";
    public const string Notes = "notes";
}

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;

    public FirebaseService(IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"];
        var credentialsPath = configuration["Firebase:CredentialsPath"];

        _firestoreDb = new FirestoreDbBuilder
        {
            ProjectId = projectId,
            CredentialsPath = credentialsPath
        }.Build();
    }

    /// <summary>
    /// Acceso directo a la base. Hace falta para abrir transacciones, que es como
    /// se implementan el guard de duplicados, el umbral de confirmaciones y la
    /// inmutabilidad de la resolucion.
    /// </summary>
    public FirestoreDb Db => _firestoreDb;

    public CollectionReference GetCollection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }
}
