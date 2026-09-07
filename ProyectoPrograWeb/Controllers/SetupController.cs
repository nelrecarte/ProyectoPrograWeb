using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

/// <summary>
/// Utilidades de arranque del proyecto. No llevan [Authorize] porque hacen falta
/// antes de que exista el primer administrador; en su lugar piden la clave de
/// ApagonYa:SetupKey, que vive en user-secrets y no en el repositorio.
/// </summary>
[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly SeedService _seedService;

    public SetupController(SeedService seedService) => _seedService = seedService;

    /// <summary>Crea el catalogo inicial de zonas. Se puede llamar varias veces sin duplicar.</summary>
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

    /// <summary>
    /// Convierte en administrador a un usuario ya registrado. Es como se crea el
    /// primer admin, porque el registro siempre da el rol Ciudadano.
    /// </summary>
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
