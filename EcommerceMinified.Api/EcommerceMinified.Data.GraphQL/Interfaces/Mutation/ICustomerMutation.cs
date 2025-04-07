using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Data.GraphQL.Interfaces.Mutation;

public interface ICustomerMutation
{
    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="customer">Customer data</param>
    /// <returns>Created customer</returns>
    Task<CustomerDto> CreateCustomer(CustomerDto customer);

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="customer">Customer data</param>
    /// <returns>Updated customer</returns>
    Task<CustomerDto> UpdateCustomer(CustomerDto customer);

    /// <summary>
    /// Delete a customer by id
    /// </summary>
    /// <param name="id">Customer id</param>
    Task DeleteCustomer(Guid id);
}
