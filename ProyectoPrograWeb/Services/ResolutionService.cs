using Google.Cloud.Firestore;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

/// <summary>
/// Escenario 4: el tecnico cierra el corte.
///
/// La resolucion se guarda con el id del reporte como id del documento, asi que
/// estructuralmente no puede haber dos. Ademas la transaccion revisa que no exista
/// antes de escribir: una resolucion registrada nunca se modifica ni se reemplaza.
/// </summary>
public class ResolutionService
{
    private readonly FirebaseService _firebase;

    public ResolutionService(FirebaseService firebase) => _firebase = firebase;

    private CollectionReference Reports => _firebase.GetCollection(Collections.Reports);
    private CollectionReference Resolutions => _firebase.GetCollection(Collections.Resolutions);

    public async Task<ResolutionDto> CreateAsync(string reportId, CreateResolutionDto dto, Technician technician)
    {
        var reportReference = Reports.Document(reportId);
        var resolutionReference = Resolutions.Document(reportId);

        var restoredAt = (dto.RestoredAt ?? DateTime.UtcNow).ToFirestoreUtc();

        if (restoredAt > DateTime.UtcNow.AddMinutes(5))
            throw new ValidationException(
                "La hora de restablecimiento no puede estar en el futuro", "hora_invalida");

        var resolution = await _firebase.Db.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(reportReference);

            if (!snapshot.Exists)
                throw new NotFoundException($"El reporte '{reportId}' no existe", "reporte_no_encontrado");

            var report = snapshot.ConvertTo<OutageReport>();

            var existing = await transaction.GetSnapshotAsync(resolutionReference);

            if (existing.Exists)
                throw new ConflictException(
                    "Este reporte ya tiene una resolucion registrada y las resoluciones son inmutables",
                    "resolucion_inmutable");

            if (report.Status == ReportStatus.Resuelto)
                throw new ConflictException("Ese corte ya fue resuelto", "reporte_resuelto");

            // Regla del enunciado: solo el tecnico asignado puede cerrar el reporte.
            if (report.AssignedTechnicianId != technician.Id)
                throw new ForbiddenException(
                    "Solo el tecnico asignado a este reporte puede registrar la resolucion",
                    "no_asignado");

            if (restoredAt < report.StartedAt)
                throw new ValidationException(
                    "La hora de restablecimiento no puede ser anterior al inicio del corte",
                    "hora_invalida");

            var now = DateTime.UtcNow;

            var record = new Resolution
            {
                Id = reportId,
                ReportId = reportId,
                ZoneId = report.ZoneId,
                TechnicianId = technician.Id,
                TechnicianName = technician.FullName,
                Cause = dto.Cause.Trim(),
                Detail = dto.Detail.Trim(),
                EstimatedMinutes = dto.EstimatedMinutes,
                RestoredAt = restoredAt,
                ResolutionMinutes = Math.Round((restoredAt - report.StartedAt).TotalMinutes, 2),
                CreatedAt = now
            };

            transaction.Set(resolutionReference, record);

            transaction.Update(reportReference, new Dictionary<string, object>
            {
                ["Status"] = ReportStatus.Resuelto,
                ["IsActive"] = false,
                ["ResolvedAt"] = restoredAt,
                ["UpdatedAt"] = now
            });

            return record;
        });

        return ResolutionDto.From(resolution);
    }

    public async Task<ResolutionDto> GetByReportAsync(string reportId)
    {
        var snapshot = await Resolutions.Document(reportId).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException(
                $"El reporte '{reportId}' todavia no tiene resolucion", "resolucion_no_encontrada");

        return ResolutionDto.From(snapshot.ConvertTo<Resolution>());
    }
}
