using System;
using AutoMapper;
using EcommerceMinified.Application.Services;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Interfaces.Caching;
using EcommerceMinified.Domain.Interfaces.Publishers;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Tests.Application.Services;

public class ProductServiceTests
{
    private readonly IProductService _sut;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IRedisService> _redisServiceMock;
    private Mock<IProductInfoPublisherService> _productInfoPublisherServiceMock;

    private readonly ProductDto _productDto;
    private readonly Product _product;
    private readonly Guid _productId = Guid.NewGuid();

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _redisServiceMock = new Mock<IRedisService>();
        _productInfoPublisherServiceMock = new Mock<IProductInfoPublisherService>();

        _sut = new ProductService(_unitOfWorkMock.Object, _mapperMock.Object, _redisServiceMock.Object, _productInfoPublisherServiceMock.Object);

        _productDto = new()
        {
            Id = _productId,
            Name = "Test Product",
            Price = 100.0m,
            Stock = 10
        };
        _product = new()
        {
            Id = _productId,
            Name = _productDto.Name,
            Price = _productDto.Price,
            Stock = _productDto.Stock
        };
    }

    [Fact]
    public async Task CreateProductAsync_WhenProductDoesNotExist_ShouldCreateAndReturnProduct()
    {
        
    }
}
