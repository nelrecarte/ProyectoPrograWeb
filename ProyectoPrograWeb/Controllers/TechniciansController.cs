using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

/// <summary>Gestion de tecnicos de zona. Es del panel del administrador.</summary>
[ApiController]
[Route("api/technicians")]
[Authorize]
public class TechniciansController : ControllerBase
{
    private readonly TechnicianService _technicianService;

    public TechniciansController(TechnicianService technicianService)
        => _technicianService = technicianService;

    /// <summary>Lista los tecnicos con su carga de reportes activos.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<List<TechnicianDto>>> GetAll([FromQuery] bool onlyActive = false)
        => Ok(await _technicianService.GetAllAsync(onlyActive));

    /// <summary>Datos del tecnico que esta usando la aplicacion.</summary>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Tecnico)]
    public async Task<ActionResult<TechnicianDto>> GetMe()
        => Ok(TechnicianDto.From(await _technicianService.GetByUserIdAsync(User.RequireUserId())));

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TechnicianDto>> GetById(string id)
        => Ok(TechnicianDto.From(await _technicianService.GetEntityAsync(id)));

    /// <summary>
    /// Registra un tecnico sobre un usuario que ya existe, y de paso lo promueve
    /// al rol Tecnico.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TechnicianDto>> Create([FromBody] CreateTechnicianDto dto)
    {
        var technician = await _technicianService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = technician.Id }, technician);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TechnicianDto>> Update(string id, [FromBody] UpdateTechnicianDto dto)
        => Ok(await _technicianService.UpdateAsync(id, dto));

    /// <summary>Desactiva al tecnico sin borrar su historial de reportes.</summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TechnicianDto>> Deactivate(string id)
        => Ok(await _technicianService.SetActiveAsync(id, false));

    [HttpPatch("{id}/activate")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TechnicianDto>> Activate(string id)
        => Ok(await _technicianService.SetActiveAsync(id, true));
}
