using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/orderitems")]
[ApiController]
public class OrderItemsController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public OrderItemsController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static OrderItemDto ToDto(OrderItem item) => new()
    {
        IdOrderItem = item.IdOrderItem,
        IdOrder = item.IdOrder,
        IdProduct = item.IdProduct,
        Quantity = item.Quantity,
        Price = item.Price
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.OrderItems
            .OrderBy(oi => oi.IdOrderItem)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(oi => ToDto(oi))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderItemDto>> GetById(int id)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<OrderItemDto>> Create([FromBody] CreateOrderItemDto dto)
    {
        var item = new OrderItem
        {
            IdOrder = dto.IdOrder,
            IdProduct = dto.IdProduct,
            Quantity = dto.Quantity,
            Price = dto.Price
        };

        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.IdOrderItem }, ToDto(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderItemDto dto)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null) return NotFound();

        item.IdOrder = dto.IdOrder;
        item.IdProduct = dto.IdProduct;
        item.Quantity = dto.Quantity;
        item.Price = dto.Price;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchOrderItemDto dto)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null) return NotFound();

        if (dto.IdOrder is not null)
            item.IdOrder = dto.IdOrder.Value;

        if (dto.IdProduct is not null)
            item.IdProduct = dto.IdProduct.Value;

        if (dto.Quantity is not null)
            item.Quantity = dto.Quantity.Value;

        if (dto.Price is not null)
            item.Price = dto.Price.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item is null) return NotFound();

        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
