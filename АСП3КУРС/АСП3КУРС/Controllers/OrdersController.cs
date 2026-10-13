using Microsoft.AspNetCore.Mvc;
using АСП3курс.DTOs;
using АСП3курс.Services;

namespace АСП3курс.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto, CancellationToken ct = default)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdOrder }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto, CancellationToken ct = default)
        => await _service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchOrderDto dto, CancellationToken ct = default)
        => await _service.PatchAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
