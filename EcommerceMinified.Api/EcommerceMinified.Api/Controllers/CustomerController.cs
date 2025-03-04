using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMinified.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController(ICustomerService _customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _customerService.GetCustomersAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CustomerDto customer)
    {
        var newCustomer = await _customerService.CreateCustomerAsync(customer);

        return Created(string.Empty, newCustomer);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCustomer(CustomerDto customer)
    {
        var updatedCustomer = await _customerService.UpdateCustomerAsync(customer);

        return Ok(updatedCustomer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);

        return NoContent();
    }
}
