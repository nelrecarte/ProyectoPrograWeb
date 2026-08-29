using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ3Backend.Services;

namespace ProyectoQ3Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }
}
