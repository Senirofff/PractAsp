namespace АСП3курс.DTOs;

public class CategoryDto
{
    public int IdCategory { get; set; }
    public string Name { get; set; } = null!;

    // Вложенные связанные данные (только для чтения).
    public int ProductCount { get; set; }
}
