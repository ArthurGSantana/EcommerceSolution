using System;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMinified.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService _orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetOrdersAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderDto order)
    {
        var newOrder = await _orderService.CreateOrderAsync(order);

        return Created(string.Empty, newOrder);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrder(OrderDto order)
    {
        var updatedOrder = await _orderService.UpdateOrderAsync(order);

        return Ok(updatedOrder);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        await _orderService.DeleteOrderAsync(id);

        return NoContent();
    }

    [HttpPost("Freight-Info")]
    public async Task<IActionResult> GetFreightInfo(FreightRequestDto freightRequest)
    {
        var freight = await _orderService.GetFreightInfoAsync(freightRequest);

        return Ok(freight);
    }
}
