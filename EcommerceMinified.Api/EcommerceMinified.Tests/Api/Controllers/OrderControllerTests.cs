using EcommerceMinified.Api.Controllers;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMinified.Tests.Api.Controllers
{
    public class OrderControllerTests
    {
        private readonly OrderController _sut;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Guid _productId = Guid.NewGuid();
        private readonly OrderDto _orderDto;
        private readonly List<OrderDto> _ordersList;
        private readonly FreightRequestDto _freightRequestDto;
        private readonly FreightResponseDto _freightResponseDto;

        public OrderControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _sut = new OrderController(_orderServiceMock.Object);

            // Setup test data - OrderItems
            var orderItems = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = _productId,
                    Quantity = 2,
                    Price = 19.99m
                }
            };

            // Setup test data - Order
            _orderDto = new OrderDto
            {
                Id = _orderId,
                CustomerId = _customerId,
                Total = 39.98m,
                Status = OrderStatusEnum.Pending,
                OrderDate = DateTime.UtcNow,
                Items = orderItems
            };

            // Setup test data - Orders list
            _ordersList = new List<OrderDto>
            {
                _orderDto,
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    Total = 79.99m,
                    Status = OrderStatusEnum.Shipped,
                    OrderDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            // Setup freight request/response
            _freightRequestDto = new FreightRequestDto
            {
                ProductId = _productId,
                ZipCode = "12345-678"
            };

            _freightResponseDto = new FreightResponseDto
            {
                FreightValue = 10.00m,
                DeliveryTime = "10 dias",
                DeliveryType = "Express"
            };
        }

        [Fact]
        public async Task GetOrders_ShouldReturnOkWithOrders()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.GetOrdersAsync())
                .ReturnsAsync(_ordersList);

            // Act
            var result = await _sut.GetOrders();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<List<OrderDto>>().Subject;
            returnValue.Should().HaveCount(2);
            returnValue.Should().BeEquivalentTo(_ordersList);
            _orderServiceMock.Verify(o => o.GetOrdersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOrders_WhenNoOrders_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.GetOrdersAsync())
                .ReturnsAsync(new List<OrderDto>());

            // Act
            var result = await _sut.GetOrders();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<List<OrderDto>>().Subject;
            returnValue.Should().BeEmpty();
            _orderServiceMock.Verify(o => o.GetOrdersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ShouldReturnOkWithOrder()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.GetOrderByIdAsync(_orderId))
                .ReturnsAsync(_orderDto);

            // Act
            var result = await _sut.GetOrderById(_orderId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnValue.Should().BeEquivalentTo(_orderDto);
            _orderServiceMock.Verify(o => o.GetOrderByIdAsync(_orderId), Times.Once);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ShouldThrowException()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.GetOrderByIdAsync(_orderId))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Order not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.GetOrderById(_orderId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Order not found");
            _orderServiceMock.Verify(o => o.GetOrderByIdAsync(_orderId), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_WhenOrderIsValid_ShouldReturnCreatedWithNewOrder()
        {
            // Arrange
            var inputOrder = new OrderDto
            {
                CustomerId = _customerId,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = _productId, Quantity = 2 }
                }
            };

            var createdOrder = new OrderDto
            {
                Id = _orderId,
                CustomerId = _customerId,
                Total = 39.98m,
                Status = OrderStatusEnum.Pending,
                OrderDate = DateTime.UtcNow,
                Items = inputOrder.Items
            };

            _orderServiceMock.Setup(o => o.CreateOrderAsync(inputOrder))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _sut.CreateOrder(inputOrder);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnValue.Should().BeEquivalentTo(createdOrder);
            _orderServiceMock.Verify(o => o.CreateOrderAsync(inputOrder), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_WhenOrderHasNoItems_ShouldThrowException()
        {
            // Arrange
            var emptyOrder = new OrderDto
            {
                CustomerId = _customerId,
                Items = new List<OrderItemDto>()
            };

            _orderServiceMock.Setup(o => o.CreateOrderAsync(emptyOrder))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Order must have at least one item", ErrorCodeEnum.BadRequest));

            // Act
            Func<Task> act = async () => await _sut.CreateOrder(emptyOrder);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Order must have at least one item");
            _orderServiceMock.Verify(o => o.CreateOrderAsync(emptyOrder), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_WhenOrderExists_ShouldReturnOkWithUpdatedOrder()
        {
            // Arrange
            var updateOrder = new OrderDto
            {
                Id = _orderId,
                Status = OrderStatusEnum.Shipped
            };

            var updatedOrder = new OrderDto
            {
                Id = _orderId,
                CustomerId = _customerId,
                Total = 39.98m,
                Status = OrderStatusEnum.Shipped,
                OrderDate = DateTime.UtcNow
            };

            _orderServiceMock.Setup(o => o.UpdateOrderAsync(updateOrder))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _sut.UpdateOrder(updateOrder);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnValue.Should().BeEquivalentTo(updatedOrder);
            _orderServiceMock.Verify(o => o.UpdateOrderAsync(updateOrder), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_WhenOrderDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var updateOrder = new OrderDto
            {
                Id = _orderId,
                Status = OrderStatusEnum.Shipped
            };

            _orderServiceMock.Setup(o => o.UpdateOrderAsync(updateOrder))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Order not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.UpdateOrder(updateOrder);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Order not found");
            _orderServiceMock.Verify(o => o.UpdateOrderAsync(updateOrder), Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_WhenOrderExists_ShouldReturnNoContent()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.DeleteOrderAsync(_orderId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteOrder(_orderId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _orderServiceMock.Verify(o => o.DeleteOrderAsync(_orderId), Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_WhenOrderDoesNotExist_ShouldThrowException()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.DeleteOrderAsync(_orderId))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Order not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.DeleteOrder(_orderId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Order not found");
            _orderServiceMock.Verify(o => o.DeleteOrderAsync(_orderId), Times.Once);
        }

        [Fact]
        public async Task GetFreightInfo_WhenProductExists_ShouldReturnOkWithFreightInfo()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.GetFreightInfoAsync(_freightRequestDto))
                .ReturnsAsync(_freightResponseDto);

            // Act
            var result = await _sut.GetFreightInfo(_freightRequestDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<FreightResponseDto>().Subject;
            returnValue.Should().BeEquivalentTo(_freightResponseDto);
            _orderServiceMock.Verify(o => o.GetFreightInfoAsync(_freightRequestDto), Times.Once);
        }

        [Fact]
        public async Task GetFreightInfo_WhenProductDoesNotExist_ShouldThrowException()
        {
            // Arrange
            _orderServiceMock.Setup(o => o.GetFreightInfoAsync(_freightRequestDto))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Product not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.GetFreightInfo(_freightRequestDto);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Product not found");
            _orderServiceMock.Verify(o => o.GetFreightInfoAsync(_freightRequestDto), Times.Once);
        }
    }
}
