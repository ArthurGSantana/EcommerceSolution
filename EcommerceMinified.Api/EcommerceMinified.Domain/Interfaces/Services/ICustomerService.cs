using System;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Domain.Interfaces.Services;

public interface ICustomerService
{
    Task<Customer> GetCustomerByIdAsync(Guid id);
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customer);
    Task<CustomerDto> UpdateCustomerAsync(CustomerDto customer);
    Task DeleteCustomerAsync(Guid id);
    Task<List<Customer>> GetCustomersAsync();
}
