using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Data.GraphQL.Query;

public class ProductQuery
{
    [GraphQLName("GetAllProducts")]
    [GraphQLDescription("Get all products")]
    public Task<List<ProductDto>> GetAllProducts([Service] IProductService productService)
    {
        return productService.GetProductsAsync();
    }

    [GraphQLName("GetProductById")]
    [GraphQLDescription("Get product by id")]
    public Task<ProductDto> GetProductById([Service] IProductService productService, Guid id)
    {
        return productService.GetProductByIdAsync(id);
    }
}
