using System;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Interfaces.GraphQL;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Data.GraphQL.Mutation;

public class ProductMutation(IProductService _productService) : IProductMutation
{
    [GraphQLName("CreateProduct")]
    [GraphQLDescription("Create a new product")]
    public Task<ProductDto> CreateProduct(ProductDto product)
    {
        if (product.Category is null)
        {
            product.Category = ProductCategoryEnum.Other;
        }
        
        return _productService.CreateProductAsync(product);
    }

    [GraphQLName("UpdateProduct")]
    [GraphQLDescription("Update an existing product")]
    public Task<ProductDto> UpdateProduct(ProductDto product)
    {
        return _productService.UpdateProductAsync(product);
    }

    [GraphQLName("DeleteProduct")]
    [GraphQLDescription("Delete a product by id")]
    public Task DeleteProduct(Guid id)
    {
        return _productService.DeleteProductAsync(id);
    }
}
