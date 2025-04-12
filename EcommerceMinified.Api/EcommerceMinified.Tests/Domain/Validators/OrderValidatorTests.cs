using EcommerceMinified.Domain.Validators;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Tests.Domain.Validators;

public class OrderValidatorTests
{
    private readonly OrderValidator _validator;

    public OrderValidatorTests()
    {
        _validator = new OrderValidator();
    }

    [Fact]
    public void CustomerId_ShouldBeRequired()
    {
        // Arrange
        var order = new OrderDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
            // CustomerId está vazio (Guid.Empty por padrão)
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "CustomerId" &&
            e.ErrorMessage == "Customer is required");
    }

    [Fact]
    public void Items_ShouldBeRequiredForNewOrders()
    {
        // Arrange
        var order = new OrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>() // Lista vazia
            // Id é null, então é um novo pedido
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Items" &&
            e.ErrorMessage == "Order items are required");
    }

    [Fact]
    public void Items_ShouldNotBeRequiredForExistingOrders()
    {
        // Arrange
        var order = new OrderDto
        {
            Id = Guid.NewGuid(), // Pedido existente
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>() // Lista vazia permitida para pedidos existentes
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Items");
    }

    [Fact]
    public void OrderItems_ShouldBeValidated()
    {
        // Arrange
        var order = new OrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    // ProductId está vazio
                    Quantity = 0  // Quantidade inválida
                }
            }
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeFalse();
        // Deveria conter erros para ProductId e Quantity
        result.Errors.Should().Contain(e => e.PropertyName.StartsWith("Items[0].ProductId"));
        result.Errors.Should().Contain(e => e.PropertyName.StartsWith("Items[0].Quantity"));
    }

    [Fact]
    public void MultipleInvalidOrderItems_ShouldAllBeValidated()
    {
        // Arrange
        var order = new OrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    // ProductId está vazio
                    Quantity = 1  // Válido
                },
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(), // Válido
                    Quantity = 0 // Inválido
                },
                new OrderItemDto
                {
                    // ProductId está vazio 
                    Quantity = 0 // Inválido
                }
            }
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeFalse();
        // Deveria ter erros para todos os itens inválidos
        result.Errors.Should().Contain(e => e.PropertyName == "Items[0].ProductId");
        result.Errors.Should().Contain(e => e.PropertyName == "Items[1].Quantity");
        result.Errors.Should().Contain(e => e.PropertyName == "Items[2].ProductId");
        result.Errors.Should().Contain(e => e.PropertyName == "Items[2].Quantity");
    }

    [Fact]
    public void ValidOrder_NewOrder_ShouldPassAllValidations()
    {
        // Arrange
        var order = new OrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                },
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 2
                }
            }
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidOrder_ExistingOrder_ShouldPassAllValidations()
    {
        // Arrange
        var order = new OrderDto
        {
            Id = Guid.NewGuid(), // Pedido existente
            CustomerId = Guid.NewGuid(),
            // Não precisa de itens para pedidos existentes
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void AllRulesShouldFail_WithEmptyOrder()
    {
        // Arrange
        var order = new OrderDto
        {
            // Todos os valores padrão/vazios
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        result.IsValid.Should().BeFalse();
        // Deveria ter falhas para CustomerId e Items
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }
}
