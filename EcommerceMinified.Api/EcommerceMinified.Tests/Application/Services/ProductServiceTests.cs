using System;
using System.Linq.Expressions;
using AutoMapper;
using EcommerceMinified.Application.Services;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Caching;
using EcommerceMinified.Domain.Interfaces.Commands;
using EcommerceMinified.Domain.Interfaces.Publishers;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using EcommerceMinified.MsgContracts.Command;

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

        _mapperMock.Setup(m => m.Map<Product>(_productDto))
            .Returns(_product);

        _mapperMock.Setup(m => m.Map<ProductDto>(_product))
            .Returns(_productDto);
    }

    [Fact]
    public async Task CreateProductAsync_WhenProductDoesNotExist_ShouldCreateAndReturnProduct()
    {
        _unitOfWorkMock
            .Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(default(Product));

        var result = await _sut.CreateProductAsync(_productDto);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_productDto);

        _unitOfWorkMock.Verify(u => u.ProductRepository.Insert(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
        _productInfoPublisherServiceMock.Verify(p => p.PublishProductInfo(It.IsAny<ProductInfoCommand>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WhenProductExists_ShouldThrowEcommerceMinifiedDomainExceptionAsync()
    {
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(_product);

        Func<Task> act = async () => await _sut.CreateProductAsync(_productDto);

        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Product already exists");
    }

    [Fact]
    public async Task CreateProductAsync_WhenProductIsCreated_ShouldPublishCorrectProductInfo()
    {
        IProductInfoCommand? capturedCommand = null;

        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(default(Product));

        _productInfoPublisherServiceMock
            .Setup(p => p.PublishProductInfo(It.IsAny<IProductInfoCommand>()))
            .Callback<IProductInfoCommand>(cmd => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        await _sut.CreateProductAsync(_productDto);

        capturedCommand.Should().NotBeNull();
        capturedCommand.ProductId.Should().Be(_productId);
        capturedCommand.ProductName.Should().Be(_productDto.Name);
        capturedCommand.ProductWeight.Should().BeInRange(1, 50);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductExists_ShouldDeleteProduct()
    {
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(_product);

        await _sut.DeleteProductAsync(_productId);

        _unitOfWorkMock.Verify(u => u.ProductRepository.Delete(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(default(Product));

        Func<Task> act = async () => await _sut.DeleteProductAsync(_productId);

        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExistsOnRedis_ShouldReturnProduct()
    {
        _redisServiceMock.Setup(r => r.GetAsync<Product>(_productId))
            .ReturnsAsync(_product);

        var result = await _sut.GetProductByIdAsync(_productId);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_productDto);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExistOnRedis_ShouldReturnProductFromDbAndSetInRedis()
    {
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(_product);

        _redisServiceMock.Setup(r => r.GetAsync<Product>(_productId))
            .ReturnsAsync(default(Product));

        var result = await _sut.GetProductByIdAsync(_productId);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_productDto);

        _redisServiceMock.Verify(r => r.SetAsync(_productId, _product, null), Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        _redisServiceMock.Setup(r => r.GetAsync<Product>(_productId))
            .ReturnsAsync(default(Product));

        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(default(Product));

        Func<Task> act = async () => await _sut.GetProductByIdAsync(_productId);

        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenCheckingIdPredicate_ShouldContainCorrectId()
    {
        _redisServiceMock.Setup(r => r.GetAsync<Product>(_productId))
            .ReturnsAsync(default(Product));

        Expression<Func<Product, bool>>? capturedPredicate = null;

        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .Callback<bool, object, Expression<Func<Product, bool>>>((tracking, include, predicate) =>
                capturedPredicate = predicate)
            .ReturnsAsync(_product);

        var result = await _sut.GetProductByIdAsync(_productId);

        capturedPredicate.Should().NotBeNull();

        string predicateString = capturedPredicate.ToString();
        predicateString.Should().Contain("x.Id ==");
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_productDto);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnAllProducts()
    {
        var products = new List<Product> { _product };
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAllAsync())
            .ReturnsAsync(products);
        _mapperMock.Setup(m => m.Map<List<ProductDto>>(products))
            .Returns(products.Select(_mapperMock.Object.Map<ProductDto>).ToList());

        var result = await _sut.GetProductsAsync();

        result.Should().NotBeNullOrEmpty();
        result.Should().BeEquivalentTo(products.Select(p => _mapperMock.Object.Map<ProductDto>(p)));
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductExists_ShouldUpdateAndReturnProduct()
    {
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(true, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(_product);

        var updatedProduct = new ProductDto
        {
            Id = _productId,
            Name = "Updated Product",
            Price = 150.0m,
            Stock = 5
        };

        _mapperMock.Setup(m => m.Map<ProductDto>(_product))
            .Returns(new ProductDto
            {
                Id = (Guid)updatedProduct.Id,
                Name = updatedProduct.Name,
                Price = updatedProduct.Price,
                Stock = updatedProduct.Stock
            });

        var result = await _sut.UpdateProductAsync(updatedProduct);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProduct);

        _unitOfWorkMock.Verify(u => u.ProductRepository.Update(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);

        _redisServiceMock.Verify(r => r.RemoveAsync<Product>(_productId), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(true, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(default(Product));

        Func<Task> act = async () => await _sut.UpdateProductAsync(_productDto);

        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Product not found");
    }
}
