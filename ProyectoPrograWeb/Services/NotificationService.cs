using Google.Cloud.Firestore;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Models;

namespace ProyectoQ3Backend.Services;

public class NotificationService
{
    private readonly FirebaseService _firebase;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(FirebaseService firebase, ILogger<NotificationService> logger)
    {
        _firebase = firebase;
        _logger = logger;
    }

    private CollectionReference Notifications => _firebase.GetCollection(Collections.Notifications);
    private CollectionReference Users => _firebase.GetCollection(Collections.Users);
    private CollectionReference Technicians => _firebase.GetCollection(Collections.Technicians);
    private CollectionReference Confirmations => _firebase.GetCollection(Collections.Confirmations);

    public async Task<List<NotificationDto>> GetForUserAsync(string userId, bool? onlyUnread)
    {
        var snapshot = await Notifications.WhereEqualTo("UserId", userId).GetSnapshotAsync();

        var items = snapshot.Documents.Select(d => d.ConvertTo<Notification>()).AsEnumerable();

        if (onlyUnread == true)
            items = items.Where(n => !n.IsRead);

        return items
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(NotificationDto.From)
            .ToList();
    }

    public async Task<int> UnreadCountAsync(string userId)
    {
        var snapshot = await Notifications
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("IsRead", false)
            .GetSnapshotAsync();

        return snapshot.Count;
    }

    public async Task MarkAsReadAsync(string id, string userId)
    {
        var reference = Notifications.Document(id);
        var snapshot = await reference.GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new NotFoundException($"La notificacion '{id}' no existe", "notificacion_no_encontrada");

        if (snapshot.ConvertTo<Notification>().UserId != userId)
            throw new ForbiddenException("Esa notificacion no es tuya", "no_asignado");

        await reference.UpdateAsync("IsRead", true);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var snapshot = await Notifications
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("IsRead", false)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            return;

        var batch = _firebase.Db.StartBatch();

        foreach (var document in snapshot.Documents)
            batch.Update(document.Reference, "IsRead", true);

        await batch.CommitAsync();
    }

    public Task NotifyNewReportAsync(OutageReport report) =>
        SafeAsync(async () =>
        {
            var destinatarios = new HashSet<string>(await AdminIdsAsync());

            foreach (var id in await ZoneTechnicianIdsAsync(report.ZoneId))
                destinatarios.Add(id);

            destinatarios.Remove(report.ReportedByUserId);

            await SaveAsync(destinatarios, new Notification
            {
                Type = NotificationType.ReporteNuevo,
                Title = "Nuevo corte reportado",
                Message = $"{report.ReportedByName} reporto un corte en {report.ZoneName}.",
                ReportId = report.Id,
                ZoneName = report.ZoneName
            });
        });

    public Task NotifyConfirmedAsync(OutageReport report) =>
        SafeAsync(async () =>
        {
            var destinatarios = new HashSet<string>();

            if (!string.IsNullOrEmpty(report.AssignedTechnicianId))
            {
                var tecnico = await Technicians.Document(report.AssignedTechnicianId).GetSnapshotAsync();

                if (tecnico.Exists)
                    destinatarios.Add(tecnico.ConvertTo<Technician>().UserId);
            }
            else
            {
                foreach (var id in await ZoneTechnicianIdsAsync(report.ZoneId))
                    destinatarios.Add(id);
            }

            destinatarios.Add(report.ReportedByUserId);

            await SaveAsync(destinatarios, new Notification
            {
                Type = NotificationType.ReporteConfirmado,
                Title = "Corte confirmado por la comunidad",
                Message = $"El corte de {report.ZoneName} llego a {report.ConfirmationCount} " +
                          "confirmaciones y paso a estado confirmado.",
                ReportId = report.Id,
                ZoneName = report.ZoneName
            });
        });

    public Task NotifyResolvedAsync(OutageReport report, Resolution resolution) =>
        SafeAsync(async () =>
        {
            var destinatarios = new HashSet<string> { report.ReportedByUserId };

            var confirmaciones = await Confirmations
                .WhereEqualTo("ReportId", report.Id)
                .GetSnapshotAsync();

            foreach (var document in confirmaciones.Documents)
                destinatarios.Add(document.ConvertTo<Confirmation>().UserId);

            await SaveAsync(destinatarios, new Notification
            {
                Type = NotificationType.ReporteResuelto,
                Title = "Servicio restablecido",
                Message = $"El corte de {report.ZoneName} se resolvio: {resolution.Cause}.",
                ReportId = report.Id,
                ZoneName = report.ZoneName
            });
        });

    private async Task<List<string>> AdminIdsAsync()
    {
        var snapshot = await Users.WhereEqualTo("Role", Roles.Administrador).GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<AppUser>().Id).ToList();
    }

    private async Task<List<string>> ZoneTechnicianIdsAsync(string zoneId)
    {
        var snapshot = await Technicians
            .WhereEqualTo("ZoneId", zoneId)
            .WhereEqualTo("IsActive", true)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(d => d.ConvertTo<Technician>().UserId)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    private async Task SaveAsync(IEnumerable<string> userIds, Notification plantilla)
    {
        var destinatarios = userIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

        if (destinatarios.Count == 0)
            return;

        var batch = _firebase.Db.StartBatch();
        var now = DateTime.UtcNow;

        foreach (var userId in destinatarios)
        {
            var reference = Notifications.Document();

            batch.Set(reference, new Notification
            {
                Id = reference.Id,
                UserId = userId,
                Type = plantilla.Type,
                Title = plantilla.Title,
                Message = plantilla.Message,
                ReportId = plantilla.ReportId,
                ZoneName = plantilla.ZoneName,
                IsRead = false,
                CreatedAt = now
            });
        }

        await batch.CommitAsync();
    }

    private async Task SafeAsync(Func<Task> accion)
    {
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron crear las notificaciones");
        }
    }
}
