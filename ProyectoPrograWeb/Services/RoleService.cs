using FirebaseAdmin.Auth;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>
/// Maneja el rol de un usuario en los dos lugares donde tiene que existir:
/// el perfil en Firestore (para poder listarlo y filtrarlo) y el custom claim
/// de Firebase Auth (que es lo que viaja dentro del ID token y lo que revisa
/// el backend en cada peticion).
/// </summary>
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

        // 1. El claim de Firebase: es lo que autoriza las peticiones.
        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(
            userId,
            new Dictionary<string, object> { ["role"] = role });

        // 2. El perfil en Firestore: es lo que se puede leer y listar.
        await document.UpdateAsync(new Dictionary<string, object> { ["Role"] = role });
    }

    /// <summary>
    /// Asigna el rol inicial al registrarse. Se llama desde AuthService y no debe
    /// tumbar el registro si falla, porque el usuario ya quedo creado en Firebase Auth.
    /// </summary>
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
            // El perfil en Firestore ya lleva el rol. Si el claim falla el usuario
            // puede entrar pero sin permisos, y un administrador lo corrige desde
            // PUT /api/users/{id}/role.
        }
    }
}
