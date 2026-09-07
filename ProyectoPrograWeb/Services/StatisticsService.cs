using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>
/// Alimenta el dashboard del administrador. Trae los reportes y las resoluciones
/// y agrega en memoria: con el volumen de un proyecto de clase es suficiente y se
/// evita depender de agregaciones de Firestore.
/// </summary>
public class StatisticsService
{
    private readonly FirebaseService _firebase;

    public StatisticsService(FirebaseService firebase) => _firebase = firebase;

    public async Task<StatisticsDto> GetAsync(string? zoneId = null, DateTime? from = null, DateTime? to = null)
    {
        var reportsSnapshot = await _firebase.GetCollection(Collections.Reports).GetSnapshotAsync();
        var resolutionsSnapshot = await _firebase.GetCollection(Collections.Resolutions).GetSnapshotAsync();

        var reports = reportsSnapshot.Documents.Select(d => d.ConvertTo<OutageReport>()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(zoneId))
            reports = reports.Where(r => r.ZoneId == zoneId);

        if (from.HasValue)
        {
            var since = from.Value.ToFirestoreUtc();
            reports = reports.Where(r => r.CreatedAt >= since);
        }

        if (to.HasValue)
        {
            var until = to.Value.ToFirestoreUtc();
            reports = reports.Where(r => r.CreatedAt <= until);
        }

        var list = reports.ToList();

        var resolutions = resolutionsSnapshot.Documents
            .Select(d => d.ConvertTo<Resolution>())
            .ToDictionary(r => r.ReportId);

        var stats = new StatisticsDto
        {
            TotalReports = list.Count,
            ActiveReports = list.Count(r => r.IsActive),
            ResolvedReports = list.Count(r => r.Status == ReportStatus.Resuelto),
            UnverifiedReports = list.Count(r => r.Status == ReportStatus.Nuevo)
        };

        var minutes = list
            .Where(r => resolutions.ContainsKey(r.Id))
            .Select(r => resolutions[r.Id].ResolutionMinutes)
            .ToList();

        stats.AverageResolutionMinutes = minutes.Count > 0 ? Math.Round(minutes.Average(), 2) : 0;

        // Grafico de barras: cortes por zona.
        stats.ReportsByZone = list
            .GroupBy(r => new { r.ZoneId, r.ZoneName })
            .Select(g =>
            {
                var resolved = g.Count(r => r.Status == ReportStatus.Resuelto);
                var zoneMinutes = g.Where(r => resolutions.ContainsKey(r.Id))
                    .Select(r => resolutions[r.Id].ResolutionMinutes)
                    .ToList();

                return new ZoneCountDto
                {
                    ZoneId = g.Key.ZoneId,
                    ZoneName = g.Key.ZoneName,
                    Total = g.Count(),
                    Resolved = resolved,
                    ResolvedPercentage = Percentage(resolved, g.Count()),
                    AverageResolutionMinutes = zoneMinutes.Count > 0 ? Math.Round(zoneMinutes.Average(), 2) : 0
                };
            })
            .OrderByDescending(z => z.Total)
            .ToList();

        // Grafico circular: distribucion por estado. Se listan los cuatro estados
        // aunque alguno vaya en cero, para que el grafico no cambie de forma.
        stats.ReportsByStatus = ReportStatus.Todos
            .Select(status => new StatusCountDto
            {
                Status = status,
                Total = list.Count(r => r.Status == status)
            })
            .ToList();

        stats.TechnicianPerformance = list
            .Where(r => !string.IsNullOrEmpty(r.AssignedTechnicianId))
            .GroupBy(r => new { r.AssignedTechnicianId, r.AssignedTechnicianName })
            .Select(g =>
            {
                var resolved = g.Count(r => r.Status == ReportStatus.Resuelto);
                var technicianMinutes = g.Where(r => resolutions.ContainsKey(r.Id))
                    .Select(r => resolutions[r.Id].ResolutionMinutes)
                    .ToList();

                return new TechnicianStatsDto
                {
                    TechnicianId = g.Key.AssignedTechnicianId,
                    TechnicianName = g.Key.AssignedTechnicianName,
                    Assigned = g.Count(),
                    Resolved = resolved,
                    ResolvedPercentage = Percentage(resolved, g.Count()),
                    AverageResolutionMinutes = technicianMinutes.Count > 0
                        ? Math.Round(technicianMinutes.Average(), 2)
                        : 0
                };
            })
            .OrderByDescending(t => t.Resolved)
            .ToList();

        return stats;
    }

    private static double Percentage(int part, int total) =>
        total == 0 ? 0 : Math.Round(part * 100.0 / total, 2);
}
