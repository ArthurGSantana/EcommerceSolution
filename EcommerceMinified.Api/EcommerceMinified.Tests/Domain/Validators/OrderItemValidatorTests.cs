using EcommerceMinified.Domain.Validators;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Tests.Domain.Validators;

public class OrderItemValidatorTests
{
    private readonly OrderItemValidator _validator;

    public OrderItemValidatorTests()
    {
        _validator = new OrderItemValidator();
    }

    [Fact]
    public void ProductId_ShouldBeRequired()
    {
        // Arrange
        var orderItem = new OrderItemDto
        {
            Quantity = 5
            // ProductId está vazio (Guid.Empty por padrão)
        };

        // Act
        var result = _validator.Validate(orderItem);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ProductId" &&
            e.ErrorMessage == "Product is required");
    }

    [Fact]
    public void Quantity_ShouldBeRequired()
    {
        // Arrange
        var orderItem = new OrderItemDto
        {
            ProductId = Guid.NewGuid()
            // Quantity não está definido (0 por padrão)
        };

        // Act
        var result = _validator.Validate(orderItem);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Quantity" &&
            e.ErrorMessage == "Quantity is required");
    }

    [Fact]
    public void Quantity_ShouldBeGreaterThanZero()
    {
        // Arrange
        var orderItem = new OrderItemDto
        {
            ProductId = Guid.NewGuid(),
            Quantity = 0 // Zero não é permitido
        };

        // Act
        var result = _validator.Validate(orderItem);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Quantity" &&
            e.ErrorMessage == "Quantity must be greater than 0");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Quantity_NegativeValues_ShouldBeInvalid(int quantity)
    {
        // Arrange
        var orderItem = new OrderItemDto
        {
            ProductId = Guid.NewGuid(),
            Quantity = quantity // Valores negativos
        };

        // Act
        var result = _validator.Validate(orderItem);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Quantity" &&
            e.ErrorMessage == "Quantity must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void ValidOrderItem_ShouldPassAllValidations(int quantity)
    {
        // Arrange
        var orderItem = new OrderItemDto
        {
            ProductId = Guid.NewGuid(),
            Quantity = quantity
        };

        // Act
        var result = _validator.Validate(orderItem);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void AllRulesShouldFail_WithEmptyOrderItem()
    {
        // Arrange
        var orderItem = new OrderItemDto();

        // Act
        var result = _validator.Validate(orderItem);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3); // Ambas as regras falham
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity must be greater than 0");
    }
}