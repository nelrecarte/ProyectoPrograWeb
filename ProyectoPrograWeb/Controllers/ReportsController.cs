using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Exceptions;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;
    private readonly ResolutionService _resolutionService;
    private readonly TechnicianService _technicianService;
    private readonly UserService _userService;

    public ReportsController(
        ReportService reportService,
        ResolutionService resolutionService,
        TechnicianService technicianService,
        UserService userService)
    {
        _reportService = reportService;
        _resolutionService = resolutionService;
        _technicianService = technicianService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReportDto>>> GetAll(
        [FromQuery] string? zoneId,
        [FromQuery] string? status,
        [FromQuery] string? technicianId,
        [FromQuery] bool? isActive,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var userId = User.RequireUserId();

        if (User.GetRole() == Roles.Tecnico && string.IsNullOrWhiteSpace(zoneId))
        {
            var technician = await _technicianService.GetByUserIdAsync(userId);
            zoneId = technician.ZoneId;
        }

        return Ok(await _reportService.GetAllAsync(zoneId, status, technicianId, isActive, from, to, userId));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<ReportDto>>> GetMine()
        => Ok(await _reportService.GetMineAsync(User.RequireUserId()));

    [HttpGet("zone/{zoneId}")]
    public async Task<ActionResult<List<ReportDto>>> GetByZone(string zoneId, [FromQuery] bool? isActive)
        => Ok(await _reportService.GetAllAsync(zoneId, isActive: isActive, currentUserId: User.RequireUserId()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDto>> GetById(string id)
        => Ok(await _reportService.GetByIdAsync(id, User.GetUserId()));

    [HttpPost]
    public async Task<ActionResult<ReportDto>> Create([FromBody] CreateReportDto dto)
    {
        var userId = User.RequireUserId();
        var profile = await _userService.GetProfileAsync(userId);

        var report = await _reportService.CreateAsync(dto, userId, profile.DisplayName);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<ReportDto>> Confirm(string id)
    {
        var userId = User.RequireUserId();
        var profile = await _userService.GetProfileAsync(userId);

        return Ok(await _reportService.ConfirmAsync(id, userId, profile.DisplayName));
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ReportDto>> Assign(string id, [FromBody] AssignTechnicianDto dto)
    {
        var technician = await _technicianService.GetEntityAsync(dto.TechnicianId);

        if (!technician.IsActive)
            throw new ValidationException("Ese tecnico esta desactivado", "tecnico_inactivo");

        return Ok(await _reportService.AssignTechnicianAsync(id, technician));
    }

    [HttpPost("{id}/accept")]
    [Authorize(Roles = Roles.Tecnico)]
    public async Task<ActionResult<ReportDto>> Accept(string id)
    {
        var technician = await _technicianService.GetByUserIdAsync(User.RequireUserId());
        var report = await _reportService.GetEntityAsync(id);

        if (report.ZoneId != technician.ZoneId)
            throw new ForbiddenException("Ese reporte no pertenece a tu zona de cobertura", "fuera_de_zona");

        return Ok(await _reportService.AssignTechnicianAsync(id, technician));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = $"{Roles.Tecnico},{Roles.Administrador}")]
    public async Task<ActionResult<ReportDto>> ChangeStatus(string id, [FromBody] ChangeStatusDto dto)
    {
        var isAdmin = User.IsAdmin();
        var technicianId = string.Empty;

        if (!isAdmin)
            technicianId = (await _technicianService.GetByUserIdAsync(User.RequireUserId())).Id;

        return Ok(await _reportService.ChangeStatusAsync(id, dto.Status, technicianId, isAdmin));
    }

    [HttpPost("{id}/resolution")]
    [Authorize(Roles = Roles.Tecnico)]
    public async Task<ActionResult<ResolutionDto>> Resolve(string id, [FromBody] CreateResolutionDto dto)
    {
        var technician = await _technicianService.GetByUserIdAsync(User.RequireUserId());
        return Ok(await _resolutionService.CreateAsync(id, dto, technician));
    }

    [HttpGet("{id}/resolution")]
    public async Task<ActionResult<ResolutionDto>> GetResolution(string id)
        => Ok(await _resolutionService.GetByReportAsync(id));
}
