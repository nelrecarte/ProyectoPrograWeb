using Google.Cloud.Firestore;

namespace ProyectoQ3Backend.Services;

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

    public FirestoreDb Db => _firestoreDb;

    public CollectionReference GetCollection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }
}
