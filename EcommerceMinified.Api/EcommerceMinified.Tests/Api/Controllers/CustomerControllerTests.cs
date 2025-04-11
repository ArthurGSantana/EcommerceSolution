using EcommerceMinified.Api.Controllers;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMinified.Tests.Api.Controllers
{
    public class CustomerControllerTests
    {
        private readonly CustomerController _sut;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly CustomerDto _customerDto;
        private readonly List<CustomerDto> _customersList;

        public CustomerControllerTests()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _sut = new CustomerController(_customerServiceMock.Object);

            // Setup test data
            _customerDto = new CustomerDto
            {
                Id = _customerId,
                Name = "Test Customer",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Phone = "123-456-7890",
                Image = "profile.jpg",
                Address = new AddressDto
                {
                    Street = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    ZipCode = "12345"
                }
            };

            _customersList = new List<CustomerDto>
            {
                _customerDto,
                new CustomerDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Another Customer",
                    Email = "another@example.com",
                    Password = "hashedpassword456"
                }
            };
        }

        [Fact]
        public async Task GetCustomers_ShouldReturnOkWithCustomers()
        {
            // Arrange
            _customerServiceMock.Setup(c => c.GetCustomersAsync())
                .ReturnsAsync(_customersList);

            // Act
            var result = await _sut.GetCustomers();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<List<CustomerDto>>().Subject;
            returnValue.Should().HaveCount(2);
            returnValue.Should().BeEquivalentTo(_customersList);
            _customerServiceMock.Verify(c => c.GetCustomersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCustomers_WhenNoCustomers_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _customerServiceMock.Setup(c => c.GetCustomersAsync())
                .ReturnsAsync(new List<CustomerDto>());

            // Act
            var result = await _sut.GetCustomers();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<List<CustomerDto>>().Subject;
            returnValue.Should().BeEmpty();
            _customerServiceMock.Verify(c => c.GetCustomersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCustomerById_WhenCustomerExists_ShouldReturnOkWithCustomer()
        {
            // Arrange
            _customerServiceMock.Setup(c => c.GetCustomerByIdAsync(_customerId))
                .ReturnsAsync(_customerDto);

            // Act
            var result = await _sut.GetCustomerById(_customerId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<CustomerDto>().Subject;
            returnValue.Should().BeEquivalentTo(_customerDto);
            _customerServiceMock.Verify(c => c.GetCustomerByIdAsync(_customerId), Times.Once);
        }

        [Fact]
        public async Task GetCustomerById_WhenCustomerDoesNotExist_ShouldThrowException()
        {
            // Arrange
            _customerServiceMock.Setup(c => c.GetCustomerByIdAsync(_customerId))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Customer not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.GetCustomerById(_customerId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer not found");
            _customerServiceMock.Verify(c => c.GetCustomerByIdAsync(_customerId), Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_ShouldReturnCreatedWithNewCustomer()
        {
            // Arrange
            var inputCustomer = new CustomerDto
            {
                Name = "New Customer",
                Email = "new@example.com",
                Password = "newpassword123"
            };

            var createdCustomer = new CustomerDto
            {
                Id = Guid.NewGuid(),
                Name = inputCustomer.Name,
                Email = inputCustomer.Email,
                Password = inputCustomer.Password
            };

            _customerServiceMock.Setup(c => c.CreateCustomerAsync(inputCustomer))
                .ReturnsAsync(createdCustomer);

            // Act
            var result = await _sut.CreateCustomer(inputCustomer);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<CustomerDto>().Subject;
            returnValue.Should().BeEquivalentTo(createdCustomer);
            _customerServiceMock.Verify(c => c.CreateCustomerAsync(inputCustomer), Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var inputCustomer = new CustomerDto
            {
                Name = "New Customer",
                Email = "existing@example.com",
                Password = "password123"
            };

            _customerServiceMock.Setup(c => c.CreateCustomerAsync(inputCustomer))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Email already exists", ErrorCodeEnum.BadRequest));

            // Act
            Func<Task> act = async () => await _sut.CreateCustomer(inputCustomer);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Email already exists");
            _customerServiceMock.Verify(c => c.CreateCustomerAsync(inputCustomer), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_WhenCustomerExists_ShouldReturnOkWithUpdatedCustomer()
        {
            // Arrange
            var updateCustomer = new CustomerDto
            {
                Id = _customerId,
                Name = "Updated Customer",
                Email = "updated@example.com",
                Password = "updatedpassword123"
            };

            _customerServiceMock.Setup(c => c.UpdateCustomerAsync(updateCustomer))
                .ReturnsAsync(updateCustomer);

            // Act
            var result = await _sut.UpdateCustomer(updateCustomer);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<CustomerDto>().Subject;
            returnValue.Should().BeEquivalentTo(updateCustomer);
            _customerServiceMock.Verify(c => c.UpdateCustomerAsync(updateCustomer), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_WhenCustomerDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var updateCustomer = new CustomerDto
            {
                Id = _customerId,
                Name = "Updated Customer",
                Email = "updated@example.com"
            };

            _customerServiceMock.Setup(c => c.UpdateCustomerAsync(updateCustomer))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Customer not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.UpdateCustomer(updateCustomer);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer not found");
            _customerServiceMock.Verify(c => c.UpdateCustomerAsync(updateCustomer), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_WhenCustomerExists_ShouldReturnNoContent()
        {
            // Arrange
            _customerServiceMock.Setup(c => c.DeleteCustomerAsync(_customerId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteCustomer(_customerId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _customerServiceMock.Verify(c => c.DeleteCustomerAsync(_customerId), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_WhenCustomerDoesNotExist_ShouldThrowException()
        {
            // Arrange
            _customerServiceMock.Setup(c => c.DeleteCustomerAsync(_customerId))
                .ThrowsAsync(new EcommerceMinifiedDomainException("Customer not found", ErrorCodeEnum.NotFound));

            // Act
            Func<Task> act = async () => await _sut.DeleteCustomer(_customerId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer not found");
            _customerServiceMock.Verify(c => c.DeleteCustomerAsync(_customerId), Times.Once);
        }
    }
}