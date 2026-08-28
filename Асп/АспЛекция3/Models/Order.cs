using System;
using System.Collections.Generic;

namespace АспЛекция3.Models
{
    public partial class Order
    {
        public int IdOrder { get; set; }
        public int IdUser { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;
        public string? DeliveryAddress { get; set; }
        public decimal TotalAmount { get; set; }
    }
}