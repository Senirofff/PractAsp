using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly OnlinePharmacyP824Context _context;

    public ProductsController(OnlinePharmacyP824Context context)
    {
        _context = context;
    }

    private static ProductDto ToDto(Product product) => new()
    {
        IdProduct = product.IdProduct,
        Name = product.Name,
        Price = product.Price,
        IdCategory = product.IdCategory,
        DosageForm = product.DosageForm,
        Dosage = product.Dosage,
        RequiresPrescription = product.RequiresPrescription,
        ExpirationDate = product.ExpirationDate,
        Discription = product.Discription,
        ImageFileName = product.ImageFileName,
        CreatedAt = product.CreatedAt
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var products = await _context.Products
            .OrderBy(p => p.IdProduct)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToDto(p))
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        return Ok(ToDto(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            IdCategory = dto.IdCategory,
            DosageForm = dto.DosageForm,
            Dosage = dto.Dosage,
            RequiresPrescription = dto.RequiresPrescription,
            ExpirationDate = dto.ExpirationDate,
            Discription = dto.Discription,
            ImageFileName = dto.ImageFileName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.IdProduct }, ToDto(product));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.IdCategory = dto.IdCategory;
        product.DosageForm = dto.DosageForm;
        product.Dosage = dto.Dosage;
        product.RequiresPrescription = dto.RequiresPrescription;
        product.ExpirationDate = dto.ExpirationDate;
        product.Discription = dto.Discription;
        product.ImageFileName = dto.ImageFileName;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        if (dto.Name is not null)
            product.Name = dto.Name;

        if (dto.Price is not null)
            product.Price = dto.Price.Value;

        if (dto.IdCategory is not null)
            product.IdCategory = dto.IdCategory.Value;

        if (dto.DosageForm is not null)
            product.DosageForm = dto.DosageForm;

        if (dto.Dosage is not null)
            product.Dosage = dto.Dosage;

        if (dto.RequiresPrescription is not null)
            product.RequiresPrescription = dto.RequiresPrescription.Value;

        if (dto.ExpirationDate is not null)
            product.ExpirationDate = dto.ExpirationDate;

        if (dto.Discription is not null)
            product.Discription = dto.Discription;

        if (dto.ImageFileName is not null)
            product.ImageFileName = dto.ImageFileName;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
