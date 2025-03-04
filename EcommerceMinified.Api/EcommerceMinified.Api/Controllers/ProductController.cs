using System;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMinified.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService _productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetProductsAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductDto product)
    {
        var newProduct = await _productService.CreateProductAsync(product);

        return Created(string.Empty, newProduct); 
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProduct(ProductDto product)
    {
        var updatedProduct = await _productService.UpdateProductAsync(product);

        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProductAsync(id);

        return NoContent();
    }
}
