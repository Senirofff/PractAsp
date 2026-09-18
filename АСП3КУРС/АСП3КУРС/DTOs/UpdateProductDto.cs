namespace АСП3курс.DTOs;

public class UpdateProductDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int IdCategory { get; set; }
    public string? DosageForm { get; set; }
    public string? Dosage { get; set; }
    public bool RequiresPrescription { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? Discription { get; set; }
    public string? ImageFileName { get; set; }
}
