using Google.Cloud.Firestore;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>
/// Gestion de tecnicos de zona. Registrar un tecnico tambien promueve al usuario
/// al rol Tecnico, para que no haya que hacerlo en dos pasos.
/// </summary>
public class TechnicianService
{
    private readonly FirebaseService _firebase;
    private readonly ZoneService _zoneService;
    private readonly RoleService _roleService;

    public TechnicianService(FirebaseService firebase, ZoneService zoneService, RoleService roleService)
    {
        _firebase = firebase;
        _zoneService = zoneService;
        _roleService = roleService;
    }

    private CollectionReference Technicians => _firebase.GetCollection(Collections.Technicians);

    public async Task<List<TechnicianDto>> GetAllAsync(bool onlyActive = false)
    {
        var snapshot = await Technicians.GetSnapshotAsync();

        var technicians = snapshot.Documents
            .Select(d => d.ConvertTo<Technician>())
            .Where(t => !onlyActive || t.IsActive)
            .OrderBy(t => t.FullName)
            .Select(TechnicianDto.From)
            .ToList();

        // Carga de trabajo: cuantos reportes abiertos tiene cada uno.
        var openReports = await _firebase.GetCollection(Collections.Reports)
            .WhereEqualTo("IsActive", true)
            .GetSnapshotAsync();

        var load = openReports.Documents
            .Select(d => d.ConvertTo<OutageReport>())
            .Where(r => !string.IsNullOrEmpty(r.AssignedTechnicianId))
            .GroupBy(r => r.AssignedTechnicianId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var technician in technicians)
            technician.ActiveReportCount = load.GetValueOrDefault(technician.Id, 0);

        return technicians;
    }

    public async Task<Technician> GetEntityAsync(string id)
    {
        var snapshot = await Technicians.Document(id).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException($"El tecnico '{id}' no existe", "tecnico_no_encontrado");

        return snapshot.ConvertTo<Technician>();
    }

    /// <summary>Busca el registro de tecnico que corresponde a un usuario autenticado.</summary>
    public async Task<Technician> GetByUserIdAsync(string userId)
    {
        var snapshot = await Technicians.WhereEqualTo("UserId", userId).Limit(1).GetSnapshotAsync();

        if (snapshot.Count == 0)
            throw new ForbiddenException(
                "Tu usuario no esta registrado como tecnico de zona", "tecnico_no_registrado");

        return snapshot.Documents[0].ConvertTo<Technician>();
    }

    public async Task<TechnicianDto> CreateAsync(CreateTechnicianDto dto)
    {
        var zone = await _zoneService.GetEntityAsync(dto.ZoneId);

        var userSnapshot = await _firebase.GetCollection(Collections.Users)
            .Document(dto.UserId)
            .GetSnapshotAsync();

        if (!userSnapshot.Exists)
            throw new NotFoundException(
                $"No hay un usuario registrado con el id '{dto.UserId}'. " +
                "El tecnico se tiene que registrar primero en la aplicacion.",
                "usuario_no_encontrado");

        var user = userSnapshot.ConvertTo<AppUser>();

        var duplicate = await Technicians.WhereEqualTo("UserId", dto.UserId).Limit(1).GetSnapshotAsync();

        if (duplicate.Count > 0)
            throw new ConflictException("Ese usuario ya esta registrado como tecnico", "tecnico_duplicado");

        var reference = Technicians.Document();

        var technician = new Technician
        {
            Id = reference.Id,
            UserId = dto.UserId,
            FullName = string.IsNullOrWhiteSpace(dto.FullName) ? user.DisplayName : dto.FullName.Trim(),
            Email = user.Email,
            ZoneId = zone.Id,
            ZoneName = zone.Name,
            IsAvailable = dto.IsAvailable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await reference.SetAsync(technician);

        // Promover al usuario para que sus peticiones pasen las politicas de rol.
        await _roleService.SetRoleAsync(dto.UserId, Roles.Tecnico);

        return TechnicianDto.From(technician);
    }

    public async Task<TechnicianDto> UpdateAsync(string id, UpdateTechnicianDto dto)
    {
        var technician = await GetEntityAsync(id);
        var zone = await _zoneService.GetEntityAsync(dto.ZoneId);

        technician.FullName = dto.FullName.Trim();
        technician.ZoneId = zone.Id;
        technician.ZoneName = zone.Name;
        technician.IsAvailable = dto.IsAvailable;

        await Technicians.Document(id).SetAsync(technician);
        return TechnicianDto.From(technician);
    }

    /// <summary>
    /// Baja logica: el tecnico deja de recibir reportes pero su historial queda intacto,
    /// que es exactamente lo que pide el enunciado.
    /// </summary>
    public async Task<TechnicianDto> SetActiveAsync(string id, bool isActive)
    {
        var technician = await GetEntityAsync(id);

        await Technicians.Document(id).UpdateAsync(new Dictionary<string, object>
        {
            ["IsActive"] = isActive
        });

        technician.IsActive = isActive;
        return TechnicianDto.From(technician);
    }
}
