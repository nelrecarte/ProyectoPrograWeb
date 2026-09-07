using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

/// <summary>Alimenta el dashboard y los graficos del administrador.</summary>
[ApiController]
[Route("api/statistics")]
[Authorize(Roles = Roles.Administrador)]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsService _statisticsService;

    public StatisticsController(StatisticsService statisticsService)
        => _statisticsService = statisticsService;

    /// <summary>
    /// Totales, reparto por zona y por estado, y desempeno por tecnico.
    /// Acepta los mismos filtros que la tabla de reportes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] string? zoneId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _statisticsService.GetAsync(zoneId, from, to));
}
