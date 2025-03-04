using System;
using EcommerceMinified.Domain.ViewModel.DTOs;
using FluentValidation;

namespace EcommerceMinified.Domain.Validators;

public class CustomerValidator : AbstractValidator<CustomerDto>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must have at least 6 characters")
            .When(x => x.Id == Guid.Empty);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MinimumLength(10)
            .WithMessage("Phone must have at least 10 characters");
    }
}
