using EcommerceMinified.Domain.Validators;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Tests.Domain.Validators;

public class ProductValidatorTests
{
    private readonly ProductValidator _validator;

    public ProductValidatorTests()
    {
        _validator = new ProductValidator();
    }

    [Fact]
    public void Name_ShouldBeRequired()
    {
        // Arrange
        var product = new ProductDto
        {
            Price = 10.99m,
            Stock = 5
            // Name está vazio
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage == "Name is required");
    }

    [Fact]
    public void Price_ShouldBeRequired()
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Stock = 5
            // Price é 0 por padrão
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage == "Price is required");
    }

    [Fact]
    public void Price_ShouldBeGreaterThanZero()
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Price = 0, // Preço inválido
            Stock = 5
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage == "Price must be greater than 0");
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-99.99)]
    public void Price_NegativeValues_ShouldBeInvalid(decimal price)
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Price = price,
            Stock = 5
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage == "Price must be greater than 0");
    }

    [Fact]
    public void Stock_ShouldBeRequired()
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Price = 10.99m
            // Stock é 0 por padrão
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Stock" &&
            e.ErrorMessage == "Stock is required");
    }

    [Fact]
    public void Stock_ShouldBeGreaterThanZero()
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Price = 10.99m,
            Stock = 0 // Estoque inválido
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Stock" &&
            e.ErrorMessage == "Stock must be greater than 0");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Stock_NegativeValues_ShouldBeInvalid(int stock)
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Price = 10.99m,
            Stock = stock // Estoque negativo
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Stock" &&
            e.ErrorMessage == "Stock must be greater than 0");
    }

    [Theory]
    [InlineData(0.01, 1)]
    [InlineData(10.99, 5)]
    [InlineData(999.99, 100)]
    public void ValidProduct_ShouldPassAllValidations(decimal price, int stock)
    {
        // Arrange
        var product = new ProductDto
        {
            Name = "Test Product",
            Price = price,
            Stock = stock
        };

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void AllRulesShouldFail_WithEmptyProduct()
    {
        // Arrange
        var product = new ProductDto();

        // Act
        var result = _validator.Validate(product);

        // Assert
        result.IsValid.Should().BeFalse();
        // Deveria ter falhas para Name, Price e Stock
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
        result.Errors.Should().Contain(e => e.PropertyName == "Stock");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Price must be greater than 0");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Stock must be greater than 0");
    }
}
