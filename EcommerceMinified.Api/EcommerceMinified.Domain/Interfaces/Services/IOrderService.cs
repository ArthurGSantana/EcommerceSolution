using System;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Domain.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(Guid id);
    Task<OrderDto> CreateOrderAsync(OrderDto order);
    Task<OrderDto> UpdateOrderAsync(OrderDto order);
    Task DeleteOrderAsync(Guid id);
    Task<List<OrderDto>> GetOrdersAsync();
    Task<FreightResponseDto> GetFreightInfoAsync(FreightRequestDto freightRequest);
}
