using EcommerceMinified.Data.Postgres.Context;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Interfaces.GraphQL;

namespace EcommerceMinified.Data.GraphQL.Query;

[ExtendObjectType("Query")]
public class CustomerQuery(PostgresDbContext _context) : ICustomerQuery
{
    [GraphQLName("GetAllCustomers")]
    [GraphQLDescription("Get all customers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetAllCustomers()
    {
        return _context.Set<Customer>().AsQueryable();
    }

    [GraphQLName("GetCustomerById")]
    [GraphQLDescription("Get customer by id")]
    public Task<Customer> GetCustomerById(Guid id)
    {
        throw new NotImplementedException();
    }
}
