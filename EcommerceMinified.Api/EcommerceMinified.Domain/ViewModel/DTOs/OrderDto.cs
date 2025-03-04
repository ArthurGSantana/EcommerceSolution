using System;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Enum;

namespace EcommerceMinified.Domain.ViewModel.DTOs;

public class OrderDto : BaseDto
{
    public Guid CustomerId { get; set; }
    public decimal Total { get; set; }
    public OrderStatusEnum Status { get; set; }
    public DateTime? OrderDate { get; set; }
    public List<OrderItemDto>? Items { get; set; }
}
