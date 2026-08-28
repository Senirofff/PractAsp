using System;
using System.Collections.Generic;

namespace АспЛекция3.Models;

public partial class Cart
{
    public int IdCart { get; set; }

    public int IdUser { get; set; }

    public DateOnly CreatedDate { get; set; }

}
