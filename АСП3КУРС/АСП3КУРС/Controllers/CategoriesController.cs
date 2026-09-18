using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public CategoriesController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static CategoryDto ToDto(Category cat) => new()
    {
        IdCategory = cat.IdCategory,
        Name = cat.Name
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var list = await _context.Categories
            .OrderBy(c => c.IdCategory)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat is null) return NotFound();
        return Ok(ToDto(cat));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var cat = new Category { Name = dto.Name };

        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cat.IdCategory }, ToDto(cat));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat is null) return NotFound();

        cat.Name = dto.Name;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchCategoryDto dto)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat is null) return NotFound();

        if (dto.Name is not null)
            cat.Name = dto.Name;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat is null) return NotFound();

        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
