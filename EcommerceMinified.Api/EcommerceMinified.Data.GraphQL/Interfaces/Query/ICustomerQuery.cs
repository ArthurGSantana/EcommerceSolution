using System;
using EcommerceMinified.Data.Postgres.Context;
using EcommerceMinified.Domain.Entity;

namespace EcommerceMinified.Domain.Interfaces.GraphQL;

public interface ICustomerQuery
{
    /// <summary>
    /// Get all customer
    /// </summary>
    /// <returns>List of customer</returns>
    IQueryable<Customer> GetAllCustomers();

    /// <summary>
    /// Get customer by id
    /// </summary>
    /// <param name="id">Customer id</param>
    /// <returns>Customer</returns>
    Task<Customer> GetCustomerById(Guid id);
}
