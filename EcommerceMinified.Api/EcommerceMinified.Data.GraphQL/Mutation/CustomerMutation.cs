using System;
using EcommerceMinified.Data.GraphQL.Interfaces.Mutation;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Data.GraphQL.Mutation;

public class CustomerMutation(ICustomerService _customerService) : ICustomerMutation
{
    [GraphQLName("CreateCustomer")]
    [GraphQLDescription("Create a new customer")]
    public async Task<CustomerDto> CreateCustomer(CustomerDto customer)
    {
        return await _customerService.CreateCustomerAsync(customer);
    }

    [GraphQLName("UpdateCustomer")]
    [GraphQLDescription("Update an existing customer")]
    public async Task<CustomerDto> UpdateCustomer(CustomerDto customer)
    {
        return await _customerService.UpdateCustomerAsync(customer);
    }

    [GraphQLName("DeleteCustomer")]
    [GraphQLDescription("Delete a customer by id")]
    public async Task DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
    }
}
