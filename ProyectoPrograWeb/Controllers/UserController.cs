using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.DTOs;
using ProyectoQ3Backend.Extensions;
using ProyectoQ3Backend.Models;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe()
        => Ok(await _userService.GetAsync(User.RequireUserId()));

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe([FromBody] UpdateProfileDto dto)
        => Ok(await _userService.UpdateAsync(User.RequireUserId(), dto));

    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<List<UserProfileDto>>> GetAll([FromQuery] string? role)
        => Ok(await _userService.GetAllAsync(role));

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<UserProfileDto>> GetById(string id)
        => Ok(await _userService.GetAsync(id));

    [HttpPut("{id}/role")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<UserProfileDto>> SetRole(string id, [FromBody] AssignRoleDto dto)
        => Ok(await _userService.SetRoleAsync(id, dto.Role));
}
