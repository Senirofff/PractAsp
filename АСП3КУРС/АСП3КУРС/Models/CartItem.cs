using System;
using System.Collections.Generic;

namespace АСП3курс.Models;

public partial class CartItem
{
    public int IdCartItem { get; set; }

    public int IdCart { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    public virtual Cart IdCartNavigation { get; set; } = null!;

    public virtual Product IdProductNavigation { get; set; } = null!;
}
