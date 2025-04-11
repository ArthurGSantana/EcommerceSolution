using EcommerceMinified.Api.Controllers;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMinified.Tests.Api.Controllers
{
    public class ProductControllerTests
    {
        private readonly ProductController _sut;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Guid _productId = Guid.NewGuid();
        private readonly ProductDto _productDto;
        private readonly List<ProductDto> _productsList;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _sut = new ProductController(_productServiceMock.Object);

            // Setup test data
            _productDto = new ProductDto
            {
                Id = _productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                Stock = 10,
                Category = ProductCategoryEnum.Electronics,
                Image = "test-image.jpg"
            };

            _productsList = new List<ProductDto>
            {
                _productDto,
                new ProductDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Second Product",
                    Description = "Another Description",
                    Price = 149.99m,
                    Stock = 5,
                    Category = ProductCategoryEnum.Electronics
                }
            };
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkWithProducts()
        {
            // Arrange
            _productServiceMock.Setup(p => p.GetProductsAsync())
                .ReturnsAsync(_productsList);

            // Act
            var result = await _sut.GetProducts();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<List<ProductDto>>().Subject;
            returnValue.Should().HaveCount(2);
            returnValue.Should().BeEquivalentTo(_productsList);
            _productServiceMock.Verify(p => p.GetProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProducts_WhenNoProducts_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _productServiceMock.Setup(p => p.GetProductsAsync())
                .ReturnsAsync(new List<ProductDto>());

            // Act
            var result = await _sut.GetProducts();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<List<ProductDto>>().Subject;
            returnValue.Should().BeEmpty();
            _productServiceMock.Verify(p => p.GetProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductById_WhenProductExists_ShouldReturnOkWithProduct()
        {
            // Arrange
            _productServiceMock.Setup(p => p.GetProductByIdAsync(_productId))
                .ReturnsAsync(_productDto);

            // Act
            var result = await _sut.GetProductById(_productId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
            returnValue.Should().BeEquivalentTo(_productDto);
            _productServiceMock.Verify(p => p.GetProductByIdAsync(_productId), Times.Once);
        }

        [Fact]
        public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            _productServiceMock.Setup(p => p.GetProductByIdAsync(_productId))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.GetProductById(_productId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Product not found");
            _productServiceMock.Verify(p => p.GetProductByIdAsync(_productId), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WhenProductDoesNotExist_ShouldReturnCreatedWithNewProduct()
        {
            // Arrange
            var inputProduct = new ProductDto
            {
                Name = "New Product",
                Description = "New Description",
                Price = 199.99m,
                Stock = 15
            };

            var createdProduct = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = inputProduct.Name,
                Description = inputProduct.Description,
                Price = inputProduct.Price,
                Stock = inputProduct.Stock
            };

            _productServiceMock.Setup(p => p.CreateProductAsync(inputProduct))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _sut.CreateProduct(inputProduct);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
            returnValue.Should().BeEquivalentTo(createdProduct);
            _productServiceMock.Verify(p => p.CreateProductAsync(inputProduct), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WhenProductExists_ShouldHandleException()
        {
            // Arrange
            var inputProduct = new ProductDto
            {
                Name = "Existing Product",
                Description = "Existing Description",
            };

            _productServiceMock.Setup(p => p.CreateProductAsync(inputProduct))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Product already exists", ErrorCodeEnum.AlreadyExists));

            // Act & Assert
            await _sut.Invoking(s => s.CreateProduct(inputProduct))
                .Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Product already exists");

            _productServiceMock.Verify(p => p.CreateProductAsync(inputProduct), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductExists_ShouldReturnOkWithUpdatedProduct()
        {
            // Arrange
            var updateProduct = new ProductDto
            {
                Id = _productId,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 129.99m,
                Stock = 20
            };

            _productServiceMock.Setup(p => p.UpdateProductAsync(updateProduct))
                .ReturnsAsync(updateProduct);

            // Act
            var result = await _sut.UpdateProduct(updateProduct);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
            returnValue.Should().BeEquivalentTo(updateProduct);
            _productServiceMock.Verify(p => p.UpdateProductAsync(updateProduct), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductDoesNotExist_ShouldHandleException()
        {
            // Arrange
            var updateProduct = new ProductDto
            {
                Id = _productId,
                Name = "Updated Product"
            };

            _productServiceMock.Setup(p => p.UpdateProductAsync(updateProduct))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound));

            // Act & Assert
            await _sut.Invoking(s => s.UpdateProduct(updateProduct))
                .Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Product not found");

            _productServiceMock.Verify(p => p.UpdateProductAsync(updateProduct), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductExists_ShouldReturnNoContent()
        {
            // Arrange
            _productServiceMock.Setup(p => p.DeleteProductAsync(_productId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteProduct(_productId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _productServiceMock.Verify(p => p.DeleteProductAsync(_productId), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_ShouldHandleException()
        {
            // Arrange
            _productServiceMock.Setup(p => p.DeleteProductAsync(_productId))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound));

            // Act & Assert
            await _sut.Invoking(s => s.DeleteProduct(_productId))
                .Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Product not found");

            _productServiceMock.Verify(p => p.DeleteProductAsync(_productId), Times.Once);
        }
    }
}