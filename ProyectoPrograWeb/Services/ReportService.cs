using Google.Cloud.Firestore;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>
/// Ciclo de vida del reporte de corte. Las tres reglas del enunciado que dependen
/// de concurrencia (un solo reporte activo por zona, el umbral de confirmaciones y
/// el cierre) se resuelven con transacciones de Firestore, no con leer-y-luego-escribir.
/// </summary>
public class ReportService
{
    private readonly FirebaseService _firebase;
    private readonly ZoneService _zoneService;
    private readonly int _confirmationThreshold;

    public ReportService(FirebaseService firebase, ZoneService zoneService, IConfiguration configuration)
    {
        _firebase = firebase;
        _zoneService = zoneService;

        // El enunciado pide "al menos una confirmacion ciudadana adicional".
        _confirmationThreshold = configuration.GetValue("ApagonYa:ConfirmationThreshold", 1);
    }

    private CollectionReference Reports => _firebase.GetCollection(Collections.Reports);
    private CollectionReference Confirmations => _firebase.GetCollection(Collections.Confirmations);
    private CollectionReference Resolutions => _firebase.GetCollection(Collections.Resolutions);

    // ------------------------------------------------------------------
    // Escenario 1 y 2: crear reporte, bloqueando el duplicado de la zona
    // ------------------------------------------------------------------

    public async Task<ReportDto> CreateAsync(CreateReportDto dto, string userId, string userName)
    {
        var zone = await _zoneService.GetEntityAsync(dto.ZoneId);

        if (!zone.IsActive)
            throw new ValidationException("Esa zona esta desactivada y no acepta reportes", "zona_inactiva");

        var startedAt = (dto.StartedAt ?? DateTime.UtcNow).ToFirestoreUtc();

        if (startedAt > DateTime.UtcNow.AddMinutes(5))
            throw new ValidationException("La hora de inicio del corte no puede estar en el futuro", "hora_invalida");

        var reference = Reports.Document();

        var report = new OutageReport
        {
            Id = reference.Id,
            ZoneId = zone.Id,
            ZoneName = zone.Name,
            Address = dto.Address.Trim(),
            StartedAt = startedAt,
            Status = ReportStatus.Nuevo,
            IsActive = true,
            ReportedByUserId = userId,
            ReportedByName = userName,
            EvidenceUrl = dto.EvidenceUrl.Trim(),
            ConfirmationCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Dos filtros de igualdad: Firestore los resuelve con los indices de un solo
        // campo que crea solo, sin necesidad de un indice compuesto.
        var activeInZone = Reports
            .WhereEqualTo("ZoneId", zone.Id)
            .WhereEqualTo("IsActive", true)
            .Limit(1);

        await _firebase.Db.RunTransactionAsync(async transaction =>
        {
            var existing = await transaction.GetSnapshotAsync(activeInZone);

            if (existing.Count > 0)
            {
                var open = existing.Documents[0].ConvertTo<OutageReport>();

                // Todavia no se ha escrito nada, asi que se puede seguir leyendo.
                var mine = await transaction.GetSnapshotAsync(
                    Confirmations.Document(Confirmation.BuildId(open.Id, userId)));

                throw new ConflictException(
                    $"La zona '{zone.Name}' ya tiene un reporte de corte abierto. " +
                    "En lugar de crear otro, confirma el que ya existe.",
                    "reporte_duplicado",
                    new DuplicateReportDto
                    {
                        ExistingReportId = open.Id,
                        ZoneId = open.ZoneId,
                        ZoneName = open.ZoneName,
                        Status = open.Status,
                        ConfirmationCount = open.ConfirmationCount,
                        AlreadyConfirmedByMe = mine.Exists || open.ReportedByUserId == userId
                    });
            }

            transaction.Set(reference, report);
            return true;
        });

        return ReportDto.From(report);
    }

    // ------------------------------------------------------------------
    // Escenario 3: confirmacion comunitaria
    // ------------------------------------------------------------------

    public async Task<ReportDto> ConfirmAsync(string reportId, string userId, string userName)
    {
        var reportReference = Reports.Document(reportId);
        var confirmationReference = Confirmations.Document(Confirmation.BuildId(reportId, userId));

        var updated = await _firebase.Db.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(reportReference);

            if (!snapshot.Exists)
                throw new NotFoundException($"El reporte '{reportId}' no existe", "reporte_no_encontrado");

            var report = snapshot.ConvertTo<OutageReport>();

            if (report.Status == ReportStatus.Resuelto)
                throw new ConflictException("Ese corte ya fue resuelto", "reporte_resuelto");

            if (report.ReportedByUserId == userId)
                throw new ConflictException(
                    "Vos creaste este reporte, no podes confirmarlo tambien", "autor_del_reporte");

            var existing = await transaction.GetSnapshotAsync(confirmationReference);

            if (existing.Exists)
                throw new ConflictException("Ya confirmaste este reporte", "confirmacion_duplicada");

            var count = report.ConfirmationCount + 1;
            var status = report.Status;

            // Al alcanzar el umbral el reporte pasa solo a confirmado.
            if (count >= _confirmationThreshold && status != ReportStatus.Confirmado)
                status = ReportStatus.Confirmado;

            var now = DateTime.UtcNow;

            transaction.Set(confirmationReference, new Confirmation
            {
                Id = confirmationReference.Id,
                ReportId = reportId,
                UserId = userId,
                UserName = userName,
                CreatedAt = now
            });

            transaction.Update(reportReference, new Dictionary<string, object>
            {
                ["ConfirmationCount"] = count,
                ["Status"] = status,
                ["UpdatedAt"] = now
            });

            report.ConfirmationCount = count;
            report.Status = status;
            report.UpdatedAt = now;
            return report;
        });

        var dto = ReportDto.From(updated);
        dto.ConfirmedByMe = true;
        return dto;
    }

    // ------------------------------------------------------------------
    // Asignacion y cambios de estado (tecnico y administrador)
    // ------------------------------------------------------------------

    public async Task<ReportDto> AssignTechnicianAsync(string reportId, Technician technician)
    {
        var reference = Reports.Document(reportId);

        var updated = await _firebase.Db.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(reference);

            if (!snapshot.Exists)
                throw new NotFoundException($"El reporte '{reportId}' no existe", "reporte_no_encontrado");

            var report = snapshot.ConvertTo<OutageReport>();

            if (report.Status == ReportStatus.Resuelto)
                throw new ConflictException("No se puede reasignar un reporte ya resuelto", "reporte_resuelto");

            var now = DateTime.UtcNow;
            var status = report.Status == ReportStatus.Nuevo ? ReportStatus.EnVerificacion : report.Status;

            transaction.Update(reference, new Dictionary<string, object>
            {
                ["AssignedTechnicianId"] = technician.Id,
                ["AssignedTechnicianName"] = technician.FullName,
                ["Status"] = status,
                ["UpdatedAt"] = now
            });

            report.AssignedTechnicianId = technician.Id;
            report.AssignedTechnicianName = technician.FullName;
            report.Status = status;
            report.UpdatedAt = now;
            return report;
        });

        return ReportDto.From(updated);
    }

    public async Task<ReportDto> ChangeStatusAsync(string reportId, string status, string technicianId, bool isAdmin)
    {
        if (status != ReportStatus.EnVerificacion && status != ReportStatus.Confirmado)
            throw new ValidationException(
                "Por aca solo se puede pasar a 'en_verificacion' o 'confirmado'. " +
                "Para cerrar el reporte se registra una resolucion.",
                "estado_invalido");

        var reference = Reports.Document(reportId);

        var updated = await _firebase.Db.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(reference);

            if (!snapshot.Exists)
                throw new NotFoundException($"El reporte '{reportId}' no existe", "reporte_no_encontrado");

            var report = snapshot.ConvertTo<OutageReport>();

            if (report.Status == ReportStatus.Resuelto)
                throw new ConflictException("Ese corte ya fue resuelto", "reporte_resuelto");

            if (!isAdmin && report.AssignedTechnicianId != technicianId)
                throw new ForbiddenException(
                    "Solo el tecnico asignado puede cambiar el estado de este reporte", "no_asignado");

            var now = DateTime.UtcNow;

            transaction.Update(reference, new Dictionary<string, object>
            {
                ["Status"] = status,
                ["UpdatedAt"] = now
            });

            report.Status = status;
            report.UpdatedAt = now;
            return report;
        });

        return ReportDto.From(updated);
    }

    // ------------------------------------------------------------------
    // Consultas
    // ------------------------------------------------------------------

    public async Task<OutageReport> GetEntityAsync(string id)
    {
        var snapshot = await Reports.Document(id).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException($"El reporte '{id}' no existe", "reporte_no_encontrado");

        return snapshot.ConvertTo<OutageReport>();
    }

    public async Task<ReportDto> GetByIdAsync(string id, string? currentUserId)
    {
        var dto = ReportDto.From(await GetEntityAsync(id));

        var resolution = await Resolutions.Document(id).GetSnapshotAsync();
        if (resolution.Exists)
            dto.Resolution = ResolutionDto.From(resolution.ConvertTo<Resolution>());

        if (!string.IsNullOrEmpty(currentUserId))
        {
            var mine = await Confirmations.Document(Confirmation.BuildId(id, currentUserId)).GetSnapshotAsync();
            dto.ConfirmedByMe = mine.Exists;
        }

        return dto;
    }

    /// <summary>
    /// Listado con filtros opcionales. Los filtros se aplican en memoria despues de
    /// traer la coleccion: con el volumen de un proyecto de clase alcanza de sobra y
    /// evita tener que crear indices compuestos en Firestore.
    /// </summary>
    public async Task<List<ReportDto>> GetAllAsync(
        string? zoneId = null,
        string? status = null,
        string? technicianId = null,
        bool? isActive = null,
        DateTime? from = null,
        DateTime? to = null,
        string? currentUserId = null)
    {
        var snapshot = await Reports.GetSnapshotAsync();

        var reports = snapshot.Documents.Select(d => d.ConvertTo<OutageReport>()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(zoneId))
            reports = reports.Where(r => r.ZoneId == zoneId);

        if (!string.IsNullOrWhiteSpace(status))
            reports = reports.Where(r => r.Status == status);

        if (!string.IsNullOrWhiteSpace(technicianId))
            reports = reports.Where(r => r.AssignedTechnicianId == technicianId);

        if (isActive.HasValue)
            reports = reports.Where(r => r.IsActive == isActive.Value);

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

        var result = reports
            .OrderByDescending(r => r.CreatedAt)
            .Select(ReportDto.From)
            .ToList();

        await MarkConfirmedByMeAsync(result, currentUserId);
        return result;
    }

    public async Task<List<ReportDto>> GetMineAsync(string userId)
    {
        var all = await GetAllAsync(currentUserId: userId);
        return all.Where(r => r.ReportedByUserId == userId).ToList();
    }

    /// <summary>Marca cuales reportes ya confirmo el usuario, para que el boton "a mi tambien" salga desactivado.</summary>
    private async Task MarkConfirmedByMeAsync(List<ReportDto> reports, string? userId)
    {
        if (string.IsNullOrEmpty(userId) || reports.Count == 0)
            return;

        var snapshot = await Confirmations.WhereEqualTo("UserId", userId).GetSnapshotAsync();

        var confirmedIds = snapshot.Documents
            .Select(d => d.ConvertTo<Confirmation>().ReportId)
            .ToHashSet();

        foreach (var report in reports)
        {
            report.ConfirmedByMe = confirmedIds.Contains(report.Id);

            if (report.ReportedByUserId == userId)
                report.ConfirmedByMe = true;
        }
    }
}
