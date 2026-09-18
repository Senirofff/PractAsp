using System;
using System.Collections.Generic;

namespace АСП3курс.Models;

public partial class Cart
{
    public int IdCart { get; set; }

    public int IdUser { get; set; }

    public DateOnly CreatedDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User IdUserNavigation { get; set; } = null!;
}
