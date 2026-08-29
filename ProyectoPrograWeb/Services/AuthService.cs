using FirebaseAdmin.Auth;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

public class AuthService
{
    private const string Collection = "users";

    private readonly FirebaseAuthClient _authClient;
    private readonly FirebaseService _firebaseService;

    public AuthService(FirebaseAuthClient authClient, FirebaseService firebaseService)
    {
        _authClient = authClient;
        _firebaseService = firebaseService;
    }

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
            Role = Roles.Usuario,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _firebaseService.GetCollection(Collection)
                .Document(profile.Id)
                .SetAsync(profile);
        }
        catch
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(credentials.LocalId);
            throw;
        }

        return ToResponse(credentials);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var credentials = await _authClient.SignInAsync(dto.Email, dto.Password);
        return ToResponse(credentials);
    }

    private static AuthResponseDto ToResponse(FirebaseAuthResult credentials) => new()
    {
        IdToken = credentials.IdToken,
        LocalId = credentials.LocalId,
        Email = credentials.Email
    };
}
