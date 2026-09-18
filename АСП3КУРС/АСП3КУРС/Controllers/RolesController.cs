using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/roles")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public RolesController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static RoleDto ToDto(Role role) => new()
    {
        IdRole = role.IdRole,
        Title = role.Title
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.Roles
            .OrderBy(r => r.IdRole)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => ToDto(r))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role is null) return NotFound();
        return Ok(ToDto(role));
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto dto)
    {
        var role = new Role { Title = dto.Title };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = role.IdRole }, ToDto(role));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role is null) return NotFound();

        role.Title = dto.Title;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchRoleDto dto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role is null) return NotFound();

        if (dto.Title is not null)
            role.Title = dto.Title;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role is null) return NotFound();

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
