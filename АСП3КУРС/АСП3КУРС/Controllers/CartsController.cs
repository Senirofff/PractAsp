using Microsoft.AspNetCore.Mvc;
using АСП3курс.DTOs;
using АСП3курс.Services;

namespace АСП3курс.Controllers;

[Route("api/carts")]
[ApiController]
public class CartsController : ControllerBase
{
    private readonly ICartService _service;

    public CartsController(ICartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CartDto>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CartDto>> Create([FromBody] CreateCartDto dto, CancellationToken ct = default)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdCart }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCartDto dto, CancellationToken ct = default)
        => await _service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchCartDto dto, CancellationToken ct = default)
        => await _service.PatchAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
