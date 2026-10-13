namespace АСП3курс.Services.Exceptions;

/// <summary>Нарушено бизнес-правило (например, не хватает товара на складе). На уровне API — 409.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
