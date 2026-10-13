namespace АСП3курс.DTOs;

public class UpdateReservationDto
{
    public int IdUser { get; set; }
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
}
