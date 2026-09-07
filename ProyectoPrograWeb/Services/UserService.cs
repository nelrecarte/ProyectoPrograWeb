using Google.Cloud.Firestore;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

public class UserService
{
    private readonly FirebaseService _firebase;
    private readonly RoleService _roleService;

    public UserService(FirebaseService firebase, RoleService roleService)
    {
        _firebase = firebase;
        _roleService = roleService;
    }

    private CollectionReference Users => _firebase.GetCollection(Collections.Users);

    public async Task<AppUser> GetProfileAsync(string userId)
    {
        var snapshot = await Users.Document(userId).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException("El perfil del usuario no existe", "usuario_no_encontrado");

        return snapshot.ConvertTo<AppUser>();
    }

    public async Task<UserProfileDto> GetAsync(string userId) =>
        UserProfileDto.From(await GetProfileAsync(userId));

    public async Task<List<UserProfileDto>> GetAllAsync(string? role = null)
    {
        var snapshot = await Users.GetSnapshotAsync();

        return snapshot.Documents
            .Select(d => d.ConvertTo<AppUser>())
            .Where(u => string.IsNullOrWhiteSpace(role) || u.Role == role)
            .OrderBy(u => u.DisplayName)
            .Select(UserProfileDto.From)
            .ToList();
    }

    public async Task<UserProfileDto> UpdateAsync(string userId, UpdateProfileDto dto)
    {
        var user = await GetProfileAsync(userId);

        user.DisplayName = dto.DisplayName.Trim();
        user.PhoneNumber = dto.PhoneNumber.Trim();
        user.Country = dto.Country.Trim();
        user.Bio = dto.Bio.Trim();
        user.ZoneId = dto.ZoneId.Trim();

        await Users.Document(userId).SetAsync(user);
        return UserProfileDto.From(user);
    }

    /// <summary>Cambia el rol en Firestore y en el custom claim de Firebase Auth.</summary>
    public async Task<UserProfileDto> SetRoleAsync(string userId, string role)
    {
        await _roleService.SetRoleAsync(userId, role);
        return await GetAsync(userId);
    }
}
