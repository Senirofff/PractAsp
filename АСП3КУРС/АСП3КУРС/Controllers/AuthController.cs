using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using АСП3курс.DTOs;
using АСП3курс.Security;
using АСП3курс.Services;

namespace АСП3курс.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokenDto>> Register(RegisterDto dto, CancellationToken ct)
    {
        var token = await service.RegisterAsync(dto, ct);
        return token is null ? Conflict(new { message = "Логин уже занят." }) : Ok(token);
    }

    [HttpPost("login")]
    [EnableRateLimiting(Access.LoginLimit)]
    public async Task<ActionResult<AccessTokenDto>> Login(LoginDto dto, CancellationToken ct)
    {
        var token = await service.LoginAsync(dto, ct);
        return token is null ? Unauthorized(new { message = "Неверные учётные данные или профиль отключён." }) : Ok(token);
    }
}
