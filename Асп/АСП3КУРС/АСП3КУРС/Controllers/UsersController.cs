using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public UsersController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static UserDto ToDto(User user) => new()
    {
        IdUser = user.IdUser,
        Login = user.Login,
        PasswordHash = user.PasswordHash,
        IdRole = user.IdRole,
        LastName = user.LastName,
        FirstName = user.FirstName,
        MiddleName = user.MiddleName,
        Phone = user.Phone,
        Email = user.Email,
        IsActive = user.IsActive,
        DiscountPercent = user.DiscountPercent
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.Users
            .OrderBy(u => u.IdUser)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => ToDto(u))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var user = new User
        {
            Login = dto.Login,
            PasswordHash = dto.PasswordHash,
            IdRole = dto.IdRole,
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = dto.IsActive,
            DiscountPercent = dto.DiscountPercent
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.IdUser }, ToDto(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Login = dto.Login;
        user.PasswordHash = dto.PasswordHash;
        user.IdRole = dto.IdRole;
        user.LastName = dto.LastName;
        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.Phone = dto.Phone;
        user.Email = dto.Email;
        user.IsActive = dto.IsActive;
        user.DiscountPercent = dto.DiscountPercent;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (dto.Login is not null)
            user.Login = dto.Login;

        if (dto.PasswordHash is not null)
            user.PasswordHash = dto.PasswordHash;

        if (dto.IdRole is not null)
            user.IdRole = dto.IdRole.Value;

        if (dto.LastName is not null)
            user.LastName = dto.LastName;

        if (dto.FirstName is not null)
            user.FirstName = dto.FirstName;

        if (dto.MiddleName is not null)
            user.MiddleName = dto.MiddleName;

        if (dto.Phone is not null)
            user.Phone = dto.Phone;

        if (dto.Email is not null)
            user.Email = dto.Email;

        if (dto.IsActive is not null)
            user.IsActive = dto.IsActive.Value;

        if (dto.DiscountPercent is not null)
            user.DiscountPercent = dto.DiscountPercent.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
