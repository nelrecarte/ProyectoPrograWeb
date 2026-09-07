using FirebaseAdmin.Auth;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

public class AuthService
{
    private readonly FirebaseAuthClient _authClient;
    private readonly FirebaseService _firebaseService;
    private readonly RoleService _roleService;

    public AuthService(
        FirebaseAuthClient authClient,
        FirebaseService firebaseService,
        RoleService roleService)
    {
        _authClient = authClient;
        _firebaseService = firebaseService;
        _roleService = roleService;
    }

    /// <summary>
    /// Todos se registran como Ciudadano. Al administrador se le sube el rol con
    /// /api/setup/promote-admin y a los tecnicos al registrarlos desde el panel.
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var credentials = await _authClient.SignUpAsync(dto.Email, dto.Password);

        var profile = new AppUser
        {
            Id = credentials.LocalId,
            UserId = credentials.LocalId,
            Email = credentials.Email,
            DisplayName = dto.DisplayName,
            Username = dto.Username,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate.ToFirestoreUtc(),
            Country = dto.Country,
            Bio = dto.Bio,
            Role = Roles.Ciudadano,
            ZoneId = dto.ZoneId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _firebaseService.GetCollection(Collections.Users)
                .Document(profile.Id)
                .SetAsync(profile);
        }
        catch
        {
            // Si el perfil no se pudo guardar, se deshace el usuario de Firebase Auth
            // para que el correo quede libre y se pueda reintentar.
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(credentials.LocalId);
            throw;
        }

        // El claim es lo que autoriza las peticiones. El token que se devuelve abajo
        // se emitio antes de este claim, asi que el rol aparece hasta el siguiente login
        // o al refrescar el token en el cliente.
        await _roleService.TrySetInitialRoleAsync(credentials.LocalId, Roles.Ciudadano);

        return ToResponse(credentials, Roles.Ciudadano);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var credentials = await _authClient.SignInAsync(dto.Email, dto.Password);

        // Se lee el rol del perfil para que el frontend sepa a que panel mandar al
        // usuario sin tener que decodificar el token.
        var role = Roles.Ciudadano;

        var snapshot = await _firebaseService.GetCollection(Collections.Users)
            .Document(credentials.LocalId)
            .GetSnapshotAsync();

        if (snapshot.Exists)
            role = snapshot.ConvertTo<AppUser>().Role;

        return ToResponse(credentials, role);
    }

    private static AuthResponseDto ToResponse(FirebaseAuthResult credentials, string role) => new()
    {
        IdToken = credentials.IdToken,
        LocalId = credentials.LocalId,
        Email = credentials.Email,
        Role = role
    };
}
