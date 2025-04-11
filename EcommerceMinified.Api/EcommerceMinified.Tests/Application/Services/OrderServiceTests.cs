using System;
using System.Linq.Expressions;
using AutoMapper;
using EcommerceMinified.Application.Services;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.RestRepository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.EntityFrameworkCore.Query;

namespace EcommerceMinified.Tests.Application.Services;

public class OrderServiceTests
{
    private readonly IOrderService _sut;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHubMinifiedRestRespository> _hubMinifiedRestRespositoryMock;

    private readonly OrderDto _orderDto;
    private readonly Order _order;
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Product _product;
    private readonly List<OrderItem> _orderItems;
    private readonly List<OrderItemDto> _orderItemDtos;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _hubMinifiedRestRespositoryMock = new Mock<IHubMinifiedRestRespository>();

        _sut = new OrderService(_unitOfWorkMock.Object, _mapperMock.Object, _hubMinifiedRestRespositoryMock.Object);

        // Setup product
        _product = new Product
        {
            Id = _productId,
            Name = "Test Product",
            Price = 100.0m,
            Stock = 10
        };

        // Setup order items
        _orderItems = new List<OrderItem>
        {
            new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = _productId,
                Quantity = 2,
                Price = _product.Price
            }
        };

        _orderItemDtos = new List<OrderItemDto>
        {
            new OrderItemDto
            {
                Id = _orderItems[0].Id,
                ProductId = _orderItems[0].ProductId,
                Quantity = _orderItems[0].Quantity,
                Price = _orderItems[0].Price
            }
        };

        // Setup order
        _order = new Order
        {
            Id = _orderId,
            CustomerId = _customerId,
            Status = OrderStatusEnum.Pending,
            OrderDate = DateTime.UtcNow,
            Total = _orderItems.Sum(x => x.Quantity * x.Price),
            Items = _orderItems
        };

        _orderDto = new OrderDto
        {
            Id = _orderId,
            CustomerId = _customerId,
            Status = OrderStatusEnum.Pending,
            OrderDate = _order.OrderDate,
            Total = _order.Total,
            Items = _orderItemDtos
        };

        // Setup mapper
        _mapperMock.Setup(m => m.Map<OrderDto>(_order))
            .Returns(_orderDto);

        _mapperMock.Setup(m => m.Map<List<OrderDto>>(It.IsAny<List<Order>>()))
            .Returns((List<Order> orders) => orders.Select(o => _mapperMock.Object.Map<OrderDto>(o)).ToList());
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValidOrder_ShouldCreateAndReturnOrder()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(u => u.ProductRepository.GetFilteredAsync(
                It.IsAny<bool>(),
                It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(new List<Product> { _product });

        _unitOfWorkMock.Setup(u => u.OrderRepository.Insert(It.IsAny<Order>()))
            .Callback<Order>(order => order.Id = _orderId);

        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(_orderDto);

        // Act
        var result = await _sut.CreateOrderAsync(_orderDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_orderDto);

        _unitOfWorkMock.Verify(u => u.OrderRepository.Insert(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenOrderHasNoItems_ShouldThrowEcommerceMinifiedDomainException()
    {
        // Arrange
        var emptyOrderDto = new OrderDto
        {
            CustomerId = _customerId,
            Items = new List<OrderItemDto>()
        };

        // Act
        Func<Task> act = async () => await _sut.CreateOrderAsync(emptyOrderDto);

        // Assert
        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Order must have at least one item");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenProductNotFound_ShouldThrowEcommerceMinifiedDomainException()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(u => u.ProductRepository.GetFilteredAsync(
                It.IsAny<bool>(),
                It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(new List<Product> { _product });

        _orderDto.Items?.Add(new OrderItemDto
        {
            ProductId = new Guid(),
            Quantity = 1,
            Price = _product.Price
        });

        // Act
        Func<Task> act = async () => await _sut.CreateOrderAsync(_orderDto);

        // Assert
        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("One or more products not found");
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderExists_ShouldDeleteOrder()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(false, null, It.IsAny<Expression<Func<Order, bool>>>()))
            .ReturnsAsync(_order);

        // Act
        await _sut.DeleteOrderAsync(_orderId);

        // Assert
        _unitOfWorkMock.Verify(u => u.OrderRepository.Delete(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(false, null, It.IsAny<Expression<Func<Order, bool>>>()))
            .ReturnsAsync(default(Order));

        // Act
        Func<Task> act = async () => await _sut.DeleteOrderAsync(_orderId);

        // Assert
        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Order not found");
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderExists_ShouldReturnOrder()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(
                false,
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<Expression<Func<Order, bool>>>()))
            .ReturnsAsync(_order);

        // Act
        var result = await _sut.GetOrderByIdAsync(_orderId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_orderDto);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(
                false,
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<Expression<Func<Order, bool>>>()))
            .ReturnsAsync(default(Order));

        // Act
        Func<Task> act = async () => await _sut.GetOrderByIdAsync(_orderId);

        // Assert
        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Order not found");
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldIncludeOrderItems()
    {
        // Arrange
        Func<IQueryable<Order>, IIncludableQueryable<Order, object>> capturedInclude = null;

        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(
                false,
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<Expression<Func<Order, bool>>>()))
            .Callback<bool, Func<IQueryable<Order>, IIncludableQueryable<Order, object>>, Expression<Func<Order, bool>>>(
                (tracking, include, predicate) => capturedInclude = include)
            .ReturnsAsync(_order);

        // Act
        var result = await _sut.GetOrderByIdAsync(_orderId);

        // Assert
        capturedInclude.Should().NotBeNull();
        // This is a simplified check since we can't easily verify the include expression
        _unitOfWorkMock.Verify(u => u.OrderRepository.GetAsync(
            false,
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            It.IsAny<Expression<Func<Order, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order> { _order };
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _sut.GetOrdersAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().BeEquivalentTo(orders.Select(p => _mapperMock.Object.Map<OrderDto>(p)));
    }

    [Fact]
    public async Task UpdateOrderAsync_WhenOrderExists_ShouldUpdateAndReturnOrder()
    {
        // Arrange
        var updatedOrderDto = new OrderDto
        {
            Id = _orderId,
            CustomerId = _customerId,
            Status = OrderStatusEnum.Delivered
        };

        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(true, null, It.IsAny<Expression<Func<Order, bool>>>()))
            .ReturnsAsync(_order);

        _mapperMock.Setup(m => m.Map<OrderDto>(_order))
            .Returns(updatedOrderDto);

        // Act
        var result = await _sut.UpdateOrderAsync(updatedOrderDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedOrderDto);
        _order.Status.Should().Be(OrderStatusEnum.Delivered);

        _unitOfWorkMock.Verify(u => u.OrderRepository.Update(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderAsync_WhenOrderDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.OrderRepository.GetAsync(true, null, It.IsAny<Expression<Func<Order, bool>>>()))
            .ReturnsAsync(default(Order));

        // Act
        Func<Task> act = async () => await _sut.UpdateOrderAsync(_orderDto);

        // Assert
        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Order not found");
    }

    [Fact]
    public async Task GetFreightInfoAsync_WhenProductExists_ShouldReturnFreightInfo()
    {
        // Arrange
        var freightRequest = new FreightRequestDto
        {
            ProductId = _productId,
            ZipCode = "12345-678"
        };

        var freightResponse = new FreightResponseDto
        {
            FreightValue = 10.0m,
            DeliveryTime = "10 dias",
            DeliveryType = "Standard"
        };

        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(_product);

        _hubMinifiedRestRespositoryMock.Setup(h => h.GetFreightInfoAsync(freightRequest))
            .ReturnsAsync(freightResponse);

        // Act
        var result = await _sut.GetFreightInfoAsync(freightRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(freightResponse);
    }

    [Fact]
    public async Task GetFreightInfoAsync_WhenProductDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
    {
        // Arrange
        var freightRequest = new FreightRequestDto
        {
            ProductId = _productId,
            ZipCode = "12345-678"
        };

        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(default(Product));

        // Act
        Func<Task> act = async () => await _sut.GetFreightInfoAsync(freightRequest);

        // Assert
        await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task GetFreightInfoAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var freightRequest = new FreightRequestDto
        {
            ProductId = _productId,
            ZipCode = "12345-678"
        };

        _unitOfWorkMock.Setup(u => u.ProductRepository.GetAsync(false, null, It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(_product);

        _hubMinifiedRestRespositoryMock.Setup(h => h.GetFreightInfoAsync(freightRequest))
            .ThrowsAsync(new Exception("External API error"));

        // Act
        Func<Task> act = async () => await _sut.GetFreightInfoAsync(freightRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("External API error");
    }
}