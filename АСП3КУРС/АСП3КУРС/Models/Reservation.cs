using System;
using System.Collections.Generic;

namespace АСП3курс.Models;

/// <summary>
/// Сущность, представляющая факт бизнес-операции: кто (пользователь),
/// что (товар и количество) и когда (дата создания) зарезервировал.
/// </summary>
public partial class Reservation
{
    public int IdReservation { get; set; }

    public int IdUser { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = ReservationStatuses.Active;

    public DateTime CreatedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual Product IdProductNavigation { get; set; } = null!;
}

/// <summary>Возможные статусы записи о резервировании.</summary>
public static class ReservationStatuses
{
    public const string Active = "Active";

    public const string Released = "Released";
}
