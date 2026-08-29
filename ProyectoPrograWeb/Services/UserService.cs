using Google.Cloud.Firestore;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

public partial class UserService
{
    protected const string Collection = "users";

    private readonly FirebaseService _firebaseService;

    public UserService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    protected DocumentReference DocumentFor(string userId) =>
        _firebaseService.GetCollection(Collection).Document(userId);

    protected async Task<AppUser> GetProfileAsync(string userId)
    {
        var snapshot = await DocumentFor(userId).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new KeyNotFoundException("El perfil del usuario no existe");

        return snapshot.ConvertTo<AppUser>();
    }
}
