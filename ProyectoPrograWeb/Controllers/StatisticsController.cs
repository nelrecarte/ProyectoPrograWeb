using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize(Roles = Roles.Administrador)]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsService _statisticsService;

    public StatisticsController(StatisticsService statisticsService)
        => _statisticsService = statisticsService;

    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] string? zoneId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _statisticsService.GetAsync(zoneId, from, to));
}
