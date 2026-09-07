using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>
/// Utilidades de arranque. Existen para que el equipo pueda levantar datos de
/// prueba y crear el primer administrador sin tocar la consola de Firebase.
/// Ambas operaciones piden la clave de <c>ApagonYa:SetupKey</c>.
/// </summary>
public class SeedService
{
    private static readonly (string Name, string Sector)[] ZonasIniciales =
    [
        ("Colonia Kennedy", "Tegucigalpa"),
        ("Colonia Miraflores", "Tegucigalpa"),
        ("Barrio La Granja", "Comayaguela"),
        ("Colonia El Pedregal", "Tegucigalpa"),
        ("Residencial Las Uvas", "Tegucigalpa"),
        ("Colonia Villa Olimpica", "Tegucigalpa"),
        ("Barrio El Centro", "Comayaguela"),
        ("Colonia Los Angeles", "Tegucigalpa")
    ];

    private readonly FirebaseService _firebase;
    private readonly RoleService _roleService;
    private readonly IConfiguration _configuration;

    public SeedService(FirebaseService firebase, RoleService roleService, IConfiguration configuration)
    {
        _firebase = firebase;
        _roleService = roleService;
        _configuration = configuration;
    }

    /// <summary>Revisa la clave de setup antes de dejar hacer nada.</summary>
    public void EnsureKey(string? provided)
    {
        var expected = _configuration["ApagonYa:SetupKey"];

        if (string.IsNullOrWhiteSpace(expected))
            throw new ForbiddenException(
                "No hay ApagonYa:SetupKey configurada. Ponerla con 'dotnet user-secrets set \"ApagonYa:SetupKey\" \"<clave>\"'.",
                "setup_deshabilitado");

        if (provided != expected)
            throw new ForbiddenException("Clave de setup incorrecta", "clave_invalida");
    }

    /// <summary>Crea el catalogo inicial de zonas. Es idempotente: no duplica las que ya existen.</summary>
    public async Task<List<string>> SeedZonesAsync()
    {
        var collection = _firebase.GetCollection(Collections.Zones);
        var snapshot = await collection.GetSnapshotAsync();

        var existing = snapshot.Documents
            .Select(d => d.ConvertTo<Zone>().Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var created = new List<string>();

        foreach (var (name, sector) in ZonasIniciales)
        {
            if (existing.Contains(name))
                continue;

            var reference = collection.Document();

            await reference.SetAsync(new Zone
            {
                Id = reference.Id,
                Name = name,
                Sector = sector,
                Description = $"Zona de cobertura en {sector}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            created.Add(name);
        }

        return created;
    }

    /// <summary>
    /// Promueve a administrador a un usuario ya registrado. Es la forma de crear el
    /// primer admin, porque todos se registran como ciudadanos.
    /// </summary>
    public async Task PromoteToAdminAsync(string email)
    {
        var snapshot = await _firebase.GetCollection(Collections.Users)
            .WhereEqualTo("Email", email)
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            throw new NotFoundException(
                $"No hay un usuario registrado con el correo '{email}'. Registrarlo primero desde /api/auth/register.",
                "usuario_no_encontrado");

        var user = snapshot.Documents[0].ConvertTo<AppUser>();
        await _roleService.SetRoleAsync(user.Id, Roles.Administrador);
    }
}
