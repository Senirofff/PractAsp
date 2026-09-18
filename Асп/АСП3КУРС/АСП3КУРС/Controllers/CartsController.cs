using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/carts")]
[ApiController]
public class CartsController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public CartsController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static CartDto ToDto(Cart cart) => new()
    {
        IdCart = cart.IdCart,
        IdUser = cart.IdUser,
        CreatedDate = cart.CreatedDate
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.Carts
            .OrderBy(c => c.IdCart)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CartDto>> GetById(int id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart is null) return NotFound();
        return Ok(ToDto(cart));
    }

    [HttpPost]
    public async Task<ActionResult<CartDto>> Create([FromBody] CreateCartDto dto)
    {
        var cart = new Cart
        {
            IdUser = dto.IdUser,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cart.IdCart }, ToDto(cart));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCartDto dto)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart is null) return NotFound();

        cart.IdUser = dto.IdUser;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchCartDto dto)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart is null) return NotFound();

        if (dto.IdUser is not null)
            cart.IdUser = dto.IdUser.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart is null) return NotFound();

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
