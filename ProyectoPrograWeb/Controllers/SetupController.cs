using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly SeedService _seedService;

    public SetupController(SeedService seedService) => _seedService = seedService;

    [HttpPost("seed-zones")]
    public async Task<IActionResult> SeedZones([FromHeader(Name = "X-Setup-Key")] string? setupKey)
    {
        _seedService.EnsureKey(setupKey);
        var created = await _seedService.SeedZonesAsync();

        return Ok(new
        {
            creadas = created.Count,
            zonas = created,
            mensaje = created.Count == 0 ? "Las zonas ya existian" : "Zonas creadas"
        });
    }

    [HttpPost("promote-admin")]
    public async Task<IActionResult> PromoteAdmin(
        [FromHeader(Name = "X-Setup-Key")] string? setupKey,
        [FromQuery] string email)
    {
        _seedService.EnsureKey(setupKey);
        await _seedService.PromoteToAdminAsync(email);

        return Ok(new
        {
            mensaje = $"'{email}' ahora es Administrador. " +
                      "Tiene que cerrar sesion y volver a entrar para que el token traiga el rol nuevo."
        });
    }
}
