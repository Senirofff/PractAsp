using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/cartitems")]
[ApiController]
public class CartItemsController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public CartItemsController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static CartItemDto ToDto(CartItem item) => new()
    {
        IdCartItem = item.IdCartItem,
        IdCart = item.IdCart,
        IdProduct = item.IdProduct,
        Quantity = item.Quantity
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.CartItems
            .OrderBy(ci => ci.IdCartItem)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ci => ToDto(ci))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CartItemDto>> GetById(int id)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> Create([FromBody] CreateCartItemDto dto)
    {
        var item = new CartItem
        {
            IdCart = dto.IdCart,
            IdProduct = dto.IdProduct,
            Quantity = dto.Quantity
        };

        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.IdCartItem }, ToDto(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCartItemDto dto)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item is null) return NotFound();

        item.IdCart = dto.IdCart;
        item.IdProduct = dto.IdProduct;
        item.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchCartItemDto dto)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item is null) return NotFound();

        if (dto.IdCart is not null)
            item.IdCart = dto.IdCart.Value;

        if (dto.IdProduct is not null)
            item.IdProduct = dto.IdProduct.Value;

        if (dto.Quantity is not null)
            item.Quantity = dto.Quantity.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item is null) return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
