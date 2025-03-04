using System;
using EcommerceMinified.Domain.Enum;

namespace EcommerceMinified.Domain.Entity;

public class Order : Base
{
    public Guid CustomerId { get; set; }
    public decimal Total { get; set; }
    public OrderStatusEnum Status { get; set; }
    public DateTime? OrderDate { get; set; }
    public List<OrderItem>? Items { get; set; }
    
}
