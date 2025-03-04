using System;
using EcommerceMinified.Domain.ViewModel.DTOs;
using FluentValidation;

namespace EcommerceMinified.Domain.Validators;

public class ProductValidator : AbstractValidator<ProductDto>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.Price)
            .NotEmpty()
            .WithMessage("Price is required")
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.Stock)
            .NotEmpty()
            .WithMessage("Stock is required")
            .GreaterThan(0)
            .WithMessage("Stock must be greater than 0");
    }
}
