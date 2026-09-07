using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

/// <summary>Catalogo de zonas. Leer lo puede cualquiera con sesion; escribir solo el administrador.</summary>
[ApiController]
[Route("api/zones")]
[Authorize]
public class ZonesController : ControllerBase
{
    private readonly ZoneService _zoneService;

    public ZonesController(ZoneService zoneService) => _zoneService = zoneService;

    /// <summary>Lista las zonas. Cada zona indica si ya tiene un corte abierto.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ZoneDto>>> GetAll([FromQuery] bool onlyActive = false)
        => Ok(await _zoneService.GetAllAsync(onlyActive));

    [HttpGet("{id}")]
    public async Task<ActionResult<ZoneDto>> GetById(string id)
        => Ok(await _zoneService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ZoneDto>> Create([FromBody] CreateZoneDto dto)
    {
        var zone = await _zoneService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = zone.Id }, zone);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ZoneDto>> Update(string id, [FromBody] UpdateZoneDto dto)
        => Ok(await _zoneService.UpdateAsync(id, dto));

    /// <summary>Baja logica. La zona deja de aceptar reportes pero conserva su historial.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Deactivate(string id)
    {
        await _zoneService.DeactivateAsync(id);
        return NoContent();
    }
}
