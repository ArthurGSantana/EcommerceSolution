using System;

namespace EcommerceMinified.Domain.Entity;

public class OrderItem : Base
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
