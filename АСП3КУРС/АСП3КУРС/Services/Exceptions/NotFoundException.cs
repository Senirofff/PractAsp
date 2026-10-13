namespace АСП3курс.Services.Exceptions;

/// <summary>Запрошенный ресурс не найден. На уровне API преобразуется в 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
