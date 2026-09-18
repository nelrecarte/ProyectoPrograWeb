using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
        => _notificationService = notificationService;

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetMine([FromQuery] bool? onlyUnread)
        => Ok(await _notificationService.GetForUserAsync(User.RequireUserId(), onlyUnread));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
        => Ok(new { total = await _notificationService.UnreadCountAsync(User.RequireUserId()) });

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        await _notificationService.MarkAsReadAsync(id, User.RequireUserId());
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(User.RequireUserId());
        return NoContent();
    }
}
