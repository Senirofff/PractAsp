using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public OrdersController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static OrderDto ToDto(Order order) => new()
    {
        IdOrder = order.IdOrder,
        IdUser = order.IdUser,
        OrderDate = order.OrderDate,
        Status = order.Status,
        DeliveryAddress = order.DeliveryAddress,
        TotalAmount = order.TotalAmount
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.Orders
            .OrderBy(o => o.IdOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => ToDto(o))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();
        return Ok(ToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
    {
        var order = new Order
        {
            IdUser = dto.IdUser,
            OrderDate = DateTime.UtcNow,
            Status = dto.Status,
            DeliveryAddress = dto.DeliveryAddress,
            TotalAmount = dto.TotalAmount
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.IdOrder }, ToDto(order));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        order.IdUser = dto.IdUser;
        order.Status = dto.Status;
        order.DeliveryAddress = dto.DeliveryAddress;
        order.TotalAmount = dto.TotalAmount;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchOrderDto dto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        if (dto.IdUser is not null)
            order.IdUser = dto.IdUser.Value;

        if (dto.Status is not null)
            order.Status = dto.Status;

        if (dto.DeliveryAddress is not null)
            order.DeliveryAddress = dto.DeliveryAddress;

        if (dto.TotalAmount is not null)
            order.TotalAmount = dto.TotalAmount.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
