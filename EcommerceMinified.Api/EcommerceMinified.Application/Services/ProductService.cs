using System;
using AutoMapper;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Caching;
using EcommerceMinified.Domain.Interfaces.Publishers;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using EcommerceMinified.MsgContracts.Command;

namespace EcommerceMinified.Application.Services;

public class ProductService(IUnitOfWork _unitOfWork, IMapper _mapper, IRedisService _redisService, IProductInfoPublisherService _productInfoPublisherService) : IProductService
{
    public async Task<ProductDto> CreateProductAsync(ProductDto product)
    {
        var random = new Random();
        var exists = await _unitOfWork.ProductRepository.GetAsync(false, null, x => x.Name == product.Name);

        if (exists != null)
        {
            throw new EcommerceMinifiedDomainException("Product already exists", ErrorCodeEnum.AlreadyExists);
        }

        var newProduct = _mapper.Map<Product>(product);

        _unitOfWork.ProductRepository.Insert(newProduct);
        await _unitOfWork.CommitPostgresAsync();

        var productInfoCommand = new ProductInfoCommand
        {
            ProductId = newProduct.Id,
            ProductName = newProduct.Name,
            ProductWeight = random.Next(1, 50)
        };
        await _productInfoPublisherService.PublishProductInfo(productInfoCommand);

        return _mapper.Map<ProductDto>(newProduct);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _unitOfWork.ProductRepository.GetAsync(false, null, x => x.Id == id);

        if (product == null)
        {
            throw new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound);
        }

        _unitOfWork.ProductRepository.Delete(product);
        await _unitOfWork.CommitPostgresAsync();
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid id)
    {
        var product = await _redisService.GetAsync<Product>(id);

        if (product is null)
        {
            product = await _unitOfWork.ProductRepository.GetAsync(false, null, x => x.Id == id);

            if (product == null)
            {
                throw new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound);
            }

            await _redisService.SetAsync(id, product);
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var products = await _unitOfWork.ProductRepository.GetAllAsync();
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<ProductDto> UpdateProductAsync(ProductDto Product)
    {
        var currentProduct = await _unitOfWork.ProductRepository.GetAsync(true, null, x => x.Id == Product.Id);

        if (currentProduct == null)
        {
            throw new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound);
        }

        currentProduct.Name = Product.Name;
        currentProduct.Description = Product.Description;
        currentProduct.Price = Product.Price;
        currentProduct.Stock = Product.Stock;
        currentProduct.Image = Product.Image;

        _unitOfWork.ProductRepository.Update(currentProduct);
        await _unitOfWork.CommitPostgresAsync();

        return _mapper.Map<ProductDto>(currentProduct);
    }
}
