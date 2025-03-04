using System;

namespace EcommerceMinified.Domain.ViewModel.DTOs;

public class OrderItemDto : BaseDto
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
