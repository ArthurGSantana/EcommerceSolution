using System;
using EcommerceMinified.Data.Postgres.Context;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Domain.Interfaces.GraphQL;

public interface IProductQuery
{
    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of products</returns>
    Task<List<ProductDto>> GetAllProducts(PostgresDbContext _context);

    /// <summary>
    /// Get product by id
    /// </summary>
    /// <param name="id">Product id</param>
    /// <returns>Product</returns>
    Task<ProductDto> GetProductById(PostgresDbContext _context, Guid id);
}
