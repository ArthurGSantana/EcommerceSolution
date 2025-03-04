using System;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Domain.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(ProductDto product);
    Task<ProductDto> UpdateProductAsync(ProductDto product);
    Task DeleteProductAsync(Guid id);
    Task<List<ProductDto>> GetProductsAsync();
}
