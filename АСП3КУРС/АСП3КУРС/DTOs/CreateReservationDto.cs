namespace АСП3курс.DTOs;

/// <summary>DTO для записи: создание резервирования (ссылки на связанные сущности по Id).</summary>
public class CreateReservationDto
{
    public int IdUser { get; set; }
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
}
