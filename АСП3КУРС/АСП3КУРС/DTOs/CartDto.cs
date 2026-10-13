namespace АСП3курс.DTOs;

public class CartDto
{
    public int IdCart { get; set; }
    public int IdUser { get; set; }
    public DateOnly CreatedDate { get; set; }

    // Вложенные связанные данные (только для чтения).
    public UserDto? User { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}
