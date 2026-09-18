namespace АСП3курс.DTOs;

public class ProductDto
{
    public int IdProduct { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int IdCategory { get; set; }
    public string? DosageForm { get; set; }
    public string? Dosage { get; set; }
    public bool RequiresPrescription { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? Discription { get; set; }
    public string? ImageFileName { get; set; }
    public DateTime CreatedAt { get; set; }
}
