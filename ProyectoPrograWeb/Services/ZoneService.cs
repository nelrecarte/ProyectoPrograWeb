using Google.Cloud.Firestore;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>Catalogo de zonas o sectores de cobertura.</summary>
public class ZoneService
{
    private readonly FirebaseService _firebase;

    public ZoneService(FirebaseService firebase) => _firebase = firebase;

    private CollectionReference Zones => _firebase.GetCollection(Collections.Zones);

    public async Task<List<ZoneDto>> GetAllAsync(bool onlyActive = false)
    {
        var snapshot = await Zones.GetSnapshotAsync();

        var zones = snapshot.Documents
            .Select(d => d.ConvertTo<Zone>())
            .Where(z => !onlyActive || z.IsActive)
            .OrderBy(z => z.Name)
            .Select(ZoneDto.From)
            .ToList();

        // Marcar cuales zonas ya tienen un corte abierto, para que el formulario
        // de reporte pueda avisar antes de que el backend rechace el duplicado.
        var activeReports = await _firebase.GetCollection(Collections.Reports)
            .WhereEqualTo("IsActive", true)
            .GetSnapshotAsync();

        var activeByZone = activeReports.Documents
            .Select(d => d.ConvertTo<OutageReport>())
            .GroupBy(r => r.ZoneId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        foreach (var zone in zones)
            if (activeByZone.TryGetValue(zone.Id, out var reportId))
                zone.ActiveReportId = reportId;

        return zones;
    }

    public async Task<Zone> GetEntityAsync(string id)
    {
        var snapshot = await Zones.Document(id).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException($"La zona '{id}' no existe", "zona_no_encontrada");

        return snapshot.ConvertTo<Zone>();
    }

    public async Task<ZoneDto> GetByIdAsync(string id) => ZoneDto.From(await GetEntityAsync(id));

    public async Task<ZoneDto> CreateAsync(CreateZoneDto dto)
    {
        var reference = Zones.Document();

        var zone = new Zone
        {
            Id = reference.Id,
            Name = dto.Name.Trim(),
            Sector = dto.Sector.Trim(),
            Description = dto.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await reference.SetAsync(zone);
        return ZoneDto.From(zone);
    }

    public async Task<ZoneDto> UpdateAsync(string id, UpdateZoneDto dto)
    {
        var zone = await GetEntityAsync(id);

        zone.Name = dto.Name.Trim();
        zone.Sector = dto.Sector.Trim();
        zone.Description = dto.Description.Trim();
        zone.IsActive = dto.IsActive;

        await Zones.Document(id).SetAsync(zone);
        return ZoneDto.From(zone);
    }

    /// <summary>
    /// Baja logica. No se borra la zona porque su historial de cortes tiene que sobrevivir.
    /// </summary>
    public async Task DeactivateAsync(string id)
    {
        await GetEntityAsync(id);

        await Zones.Document(id).UpdateAsync(new Dictionary<string, object>
        {
            ["IsActive"] = false
        });
    }
}
