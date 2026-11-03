using Microsoft.AspNetCore.Authorization;
using АСП3курс.Security;
using Microsoft.AspNetCore.Mvc;
using АСП3курс.DTOs;
using АСП3курс.Services;

namespace АСП3курс.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    private readonly IAuthorizationService _authorization;

    public UsersController(IUserService service, IAuthorizationService authorization)
    {
        _service = service;
        _authorization = authorization;
    }

    [Authorize(Policy = Access.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        if (dto is null)
            return NotFound();

        if (!(await _authorization.AuthorizeAsync(User, dto, Access.Profile)).Succeeded)
            return Forbid();

        return Ok(dto);
    }

    [HttpPatch("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(int id, UpdateProfileDto dto, CancellationToken ct)
    {
        var profile = await _service.GetByIdAsync(id, ct);
        if (profile is null)
            return NotFound();

        if (!(await _authorization.AuthorizeAsync(User, profile, Access.Profile)).Succeeded)
            return Forbid();

        return await _service.UpdateProfileAsync(id, dto, ct) ? NoContent() : NotFound();
    }

    [Authorize(Policy = Access.Admin)]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto, CancellationToken ct = default)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdUser }, created);
    }

    [Authorize(Policy = Access.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto, CancellationToken ct = default)
        => await _service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();

    [Authorize(Policy = Access.Admin)]
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchUserDto dto, CancellationToken ct = default)
        => await _service.PatchAsync(id, dto, ct) ? NoContent() : NotFound();

    [Authorize(Policy = Access.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
