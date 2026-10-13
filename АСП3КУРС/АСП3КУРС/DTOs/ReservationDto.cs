namespace АСП3курс.DTOs;

/// <summary>DTO для чтения записи о резервировании (с вложенными связанными данными).</summary>
public class ReservationDto
{
    public int IdReservation { get; set; }
    public int IdUser { get; set; }
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }

    public UserDto? User { get; set; }
    public ProductDto? Product { get; set; }
}
