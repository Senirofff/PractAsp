namespace АСП3курс.DTOs;

public class CartItemDto
{
    public int IdCartItem { get; set; }
    public int IdCart { get; set; }
    public int IdProduct { get; set; }
    public int Quantity { get; set; }

    // Вложенные связанные данные (только для чтения).
    public ProductDto? Product { get; set; }
}
