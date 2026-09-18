using FirebaseAdmin.Auth;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

public class RoleService
{
    private readonly FirebaseService _firebase;

    public RoleService(FirebaseService firebase) => _firebase = firebase;

    public async Task SetRoleAsync(string userId, string role)
    {
        if (!Roles.EsValido(role))
            throw new ValidationException(
                $"Rol invalido: '{role}'. Los validos son {string.Join(", ", Roles.Todos)}",
                "rol_invalido");

        var document = _firebase.GetCollection(Collections.Users).Document(userId);
        var snapshot = await document.GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException($"El usuario '{userId}' no tiene perfil registrado", "usuario_no_encontrado");

        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(
            userId,
            new Dictionary<string, object> { ["role"] = role });

        await document.UpdateAsync(new Dictionary<string, object> { ["Role"] = role });
    }

    public async Task TrySetInitialRoleAsync(string userId, string role)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(
                userId,
                new Dictionary<string, object> { ["role"] = role });
        }
        catch (Exception)
        {
        }
    }
}
