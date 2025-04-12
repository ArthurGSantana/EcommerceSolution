using EcommerceMinified.Domain.Validators;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Tests.Domain.Validators
{
    public class CustomerValidatorTests
    {
        private readonly CustomerValidator _validator;

        public CustomerValidatorTests()
        {
            _validator = new CustomerValidator();
        }

        [Fact]
        public void Name_ShouldBeRequired()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Email = "test@example.com",
                Password = "password123",
                Phone = "1234567890"
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Name" &&
                e.ErrorMessage == "Name is required");
        }

        [Fact]
        public void Email_ShouldBeRequired()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Password = "password123",
                Phone = "1234567890"
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Email" &&
                e.ErrorMessage == "Email is required");
        }

        [Fact]
        public void Email_ShouldHaveValidFormat()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Email = "invalid-email",
                Password = "password123",
                Phone = "1234567890"
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Email" &&
                e.ErrorMessage == "Invalid email");
        }

        [Fact]
        public void Password_ShouldBeRequiredForNewCustomers()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890"
                // Id é Guid.Empty por padrão, então é um novo cliente
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Password" &&
                e.ErrorMessage == "Password is required");
        }

        [Fact]
        public void Password_ShouldNotBeRequiredForExistingCustomers()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Id = Guid.NewGuid(), // Cliente existente
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890"
                // Sem senha
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == "Password");
        }

        [Fact]
        public void Password_ShouldHaveMinimumLengthForNewCustomers()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "12345", // Apenas 5 caracteres
                Phone = "1234567890"
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Password" &&
                e.ErrorMessage == "Password must have at least 6 characters");
        }

        [Fact]
        public void Password_MinimumLengthShouldNotApplyToExistingCustomers()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Id = Guid.NewGuid(), // Cliente existente
                Name = "Test User",
                Email = "test@example.com",
                Password = "12345", // Senha curta, mas não importa para clientes existentes
                Phone = "1234567890"
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == "Password");
        }

        [Fact]
        public void Phone_ShouldBeRequired()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password123"
                // Sem telefone
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Phone" &&
                e.ErrorMessage == "Phone is required");
        }

        [Fact]
        public void Phone_ShouldHaveMinimumLength()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password123",
                Phone = "123456789" // Apenas 9 caracteres
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "Phone" &&
                e.ErrorMessage == "Phone must have at least 10 characters");
        }

        [Fact]
        public void ValidCustomer_NewCustomer_ShouldPassAllValidations()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password123",
                Phone = "1234567890"
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidCustomer_ExistingCustomer_ShouldPassAllValidations()
        {
            // Arrange
            var customer = new CustomerDto
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890"
                // Observe que não estamos fornecendo senha aqui,
                // porque não é necessário para clientes existentes
            };

            // Act
            var result = _validator.Validate(customer);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
