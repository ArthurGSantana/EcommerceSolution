using System;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Domain.Interfaces.GraphQL;

public interface IProductMutation
{
    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="product">Product data</param>
    /// <returns>Created product</returns>
    Task<ProductDto> CreateProduct(ProductDto product);

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="product">Product data</param>
    /// <returns>Updated product</returns>
    Task<ProductDto> UpdateProduct(ProductDto product);

    /// <summary>
    /// Delete a product by id
    /// </summary>
    /// <param name="id">Product id</param>
    Task DeleteProduct(Guid id);
}
