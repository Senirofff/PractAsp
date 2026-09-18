using System;
using System.Collections.Generic;

namespace АСП3курс.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int IdCategory { get; set; }

    public string? DosageForm { get; set; }

    public string? Dosage { get; set; }

    public bool RequiresPrescription { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public string? Discription { get; set; }

    public string? ImageFileName { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
