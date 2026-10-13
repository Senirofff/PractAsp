using Microsoft.AspNetCore.Mvc;
using АСП3курс.DTOs;
using АСП3курс.Services;

namespace АСП3курс.Controllers;

[Route("api/reservations")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Создание резервирования: уменьшает остаток товара и создаёт запись о событии.
    /// При нехватке товара возвращает 409, при отсутствии пользователя/товара — 404.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Reserve([FromBody] CreateReservationDto dto, CancellationToken ct = default)
    {
        var created = await _service.ReserveAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.IdReservation }, created);
    }

    /// <summary>
    /// Отмена резервирования: возвращает товар на склад и закрывает запись.
    /// Возвращает 404, если активной записи нет.
    /// </summary>
    [HttpPost("{id}/release")]
    public async Task<ActionResult<ReservationDto>> Release(int id, CancellationToken ct = default)
    {
        var released = await _service.ReleaseAsync(id, ct);
        return Ok(released);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto, CancellationToken ct = default)
        => await _service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchReservationDto dto, CancellationToken ct = default)
        => await _service.PatchAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
