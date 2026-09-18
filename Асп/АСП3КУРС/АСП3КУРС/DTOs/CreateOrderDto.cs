namespace АСП3курс.DTOs;

public class CreateOrderDto
{
    public int IdUser { get; set; }
    public string Status { get; set; } = null!;
    public string? DeliveryAddress { get; set; }
    public decimal TotalAmount { get; set; }
}
