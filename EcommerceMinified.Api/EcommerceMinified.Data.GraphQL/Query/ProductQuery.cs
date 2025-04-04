using EcommerceMinified.Domain.Interfaces.GraphQL;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Data.GraphQL.Query;

public class ProductQuery(IProductService _productService) : IProductQuery
{
    [GraphQLName("GetAllProducts")]
    [GraphQLDescription("Get all products")]
    public Task<List<ProductDto>> GetAllProducts()
    {
        return _productService.GetProductsAsync();
    }

    [GraphQLName("GetProductById")]
    [GraphQLDescription("Get product by id")]
    public Task<ProductDto> GetProductById(Guid id)
    {
        return _productService.GetProductByIdAsync(id);
    }
}
