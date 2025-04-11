using System.Linq.Expressions;
using AutoMapper;
using EcommerceMinified.Application.Services;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.EntityFrameworkCore.Query;

namespace EcommerceMinified.Tests.Application.Services
{
    public class CustomerServiceTests
    {
        private readonly ICustomerService _sut;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly CustomerDto _customerDto;
        private readonly Customer _customer;
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Address _address;
        private readonly AddressDto _addressDto;

        public CustomerServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _sut = new CustomerService(_unitOfWorkMock.Object, _mapperMock.Object);

            // Setup address
            _address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Test Street",
                Number = "123",
                City = "Test City",
                State = "TS",
                ZipCode = "12345-678",
            };

            _addressDto = new AddressDto
            {
                Id = _address.Id,
                Street = _address.Street,
                Number = _address.Number,
                City = _address.City,
                State = _address.State,
                ZipCode = _address.ZipCode,
            };

            // Setup customer
            _customer = new Customer
            {
                Id = _customerId,
                Name = "Test Customer",
                Email = "test@example.com",
                Password = "hashedPassword",
                Phone = "123-456-7890",
                Image = "profile.jpg",
                Address = _address
            };

            _customerDto = new CustomerDto
            {
                Id = _customerId,
                Name = _customer.Name,
                Email = _customer.Email,
                Password = _customer.Password,
                Phone = _customer.Phone,
                Image = _customer.Image,
                Address = _addressDto
            };

            // Setup mapper
            _mapperMock.Setup(m => m.Map<Customer>(_customerDto))
                .Returns(_customer);

            _mapperMock.Setup(m => m.Map<CustomerDto>(_customer))
                .Returns(_customerDto);

            _mapperMock.Setup(m => m.Map<List<CustomerDto>>(It.IsAny<List<Customer>>()))
                .Returns((List<Customer> customers) => customers.Select(c => _mapperMock.Object.Map<CustomerDto>(c)).ToList());
        }

        [Fact]
        public async Task CreateCustomerAsync_WhenCustomerDoesNotExist_ShouldCreateAndReturnCustomer()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(default(Customer));

            // Act
            var result = await _sut.CreateCustomerAsync(_customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(_customerDto);

            _unitOfWorkMock.Verify(u => u.CustomerRepository.Insert(It.IsAny<Customer>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_WhenCustomerExists_ShouldThrowEcommerceMinifiedDomainException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(_customer);

            // Act
            Func<Task> act = async () => await _sut.CreateCustomerAsync(_customerDto);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer already exists");
        }

        [Fact]
        public async Task DeleteCustomerAsync_WhenCustomerExists_ShouldDeleteCustomer()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(_customer);

            // Act
            await _sut.DeleteCustomerAsync(_customerId);

            // Assert
            _unitOfWorkMock.Verify(u => u.CustomerRepository.Delete(It.IsAny<Customer>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomerAsync_WhenCustomerDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(default(Customer));

            // Act
            Func<Task> act = async () => await _sut.DeleteCustomerAsync(_customerId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer not found");
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(_customer);

            // Act
            var result = await _sut.GetCustomerByIdAsync(_customerId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(_customerId);
            result.Email.Should().Be(_customer.Email);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WhenIncludingAddress_ShouldSetupCorrectIncludeExpression()
        {
            // Arrange
            Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>? capturedInclude = null;

            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .Callback<bool, Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>, Expression<Func<Customer, bool>>>(
                    (tracking, include, predicate) => capturedInclude = include)
                .ReturnsAsync(_customer);

            // Act
            var result = await _sut.GetCustomerByIdAsync(_customerId);

            // Assert
            capturedInclude.Should().NotBeNull();

            _unitOfWorkMock.Verify(u => u.CustomerRepository.GetAsync(
                false,
                It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                It.Is<Expression<Func<Customer, bool>>>(e => e.ToString().Contains("x.Id"))),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WhenCustomerDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(default(Customer));

            // Act
            Func<Task> act = async () => await _sut.GetCustomerByIdAsync(_customerId);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer not found");
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldReturnAllCustomers()
        {
            // Arrange
            var customers = new List<Customer> { _customer };

            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetFilteredAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>(),
                    It.IsAny<Func<IQueryable<Customer>, IOrderedQueryable<Customer>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetCustomersAsync();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(_customerId);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldSetupCorrectIncludeExpression()
        {
            // Arrange
            Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>? capturedInclude = null;

            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetFilteredAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>(),
                    It.IsAny<Func<IQueryable<Customer>, IOrderedQueryable<Customer>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .Callback<bool, Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>, Expression<Func<Customer, bool>>,
                    Func<IQueryable<Customer>, IOrderedQueryable<Customer>>, int?, int?>(
                        (tracking, include, predicate, orderBy, page, perPage) => capturedInclude = include)
                .ReturnsAsync(new List<Customer> { _customer });

            // Act
            var result = await _sut.GetCustomersAsync();

            // Assert
            capturedInclude.Should().NotBeNull();

            _unitOfWorkMock.Verify(u => u.CustomerRepository.GetFilteredAsync(
                false,
                It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                null,
                null,
                null,
                null),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerAsync_WhenCustomerExists_ShouldUpdateAndReturnCustomer()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(_customer);

            var updatedCustomerDto = new CustomerDto
            {
                Id = _customerId,
                Name = "Updated Name",
                Email = "updated@example.com",
                Phone = "987-654-3210",
                Image = "updated.jpg"
            };

            // Act
            var result = await _sut.UpdateCustomerAsync(updatedCustomerDto);

            // Assert
            _customer.Name.Should().Be("Updated Name");
            _customer.Email.Should().Be("updated@example.com");
            _customer.Phone.Should().Be("987-654-3210");
            _customer.Image.Should().Be("updated.jpg");

            _unitOfWorkMock.Verify(u => u.CustomerRepository.Update(It.IsAny<Customer>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitPostgresAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerAsync_WhenCustomerDoesNotExist_ShouldThrowEcommerceMinifiedDomainException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAsync(
                    It.IsAny<bool>(),
                    It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                    It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(default(Customer));

            // Act
            Func<Task> act = async () => await _sut.UpdateCustomerAsync(_customerDto);

            // Assert
            await act.Should().ThrowAsync<EcommerceMinifiedDomainException>()
                .WithMessage("Customer not found");
        }
    }
}