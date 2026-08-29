using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        => Ok(await _authService.RegisterAsync(dto));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        => Ok(await _authService.LoginAsync(dto));
}
