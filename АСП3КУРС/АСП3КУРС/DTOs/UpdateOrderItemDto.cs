namespace АСП3курс.DTOs;

public class UpdateOrderItemDto
{
    public int IdOrder { get; set; }
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
