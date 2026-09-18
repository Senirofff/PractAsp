namespace АСП3курс.DTOs;

public class OrderItemDto
{
    public int IdOrderItem { get; set; }
    public int IdOrder { get; set; }
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
