using EcommerceMinified.Data.Postgres.Context;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.GraphQL;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Data.GraphQL.Query;

[ExtendObjectType("Query")]
public class ProductQuery(IProductService _productService) : IProductQuery //usando service como base de dados
{
    [GraphQLName("GetAllProducts")]
    [GraphQLDescription("Get all products")]
    public async Task<List<ProductDto>> GetAllProducts([Service] PostgresDbContext _context)
    {
        var products = await _productService.GetProductsAsync();

        if (products is null || products.Count.Equals(0))
        {
            throw new EcommerceMinifiedDomainException("No products found.", ErrorCodeEnum.NotFound);
        }

        return products;
    }

    [GraphQLName("GetProductById")]
    [GraphQLDescription("Get product by id")]
    public Task<ProductDto> GetProductById([Service] PostgresDbContext _context, Guid id)
    {
        var product = _productService.GetProductByIdAsync(id);

        if (product is null)
        {
            throw new EcommerceMinifiedDomainException("Product not found.", ErrorCodeEnum.NotFound);
        }

        return product;
    }
}
