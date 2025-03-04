using System;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.ViewModel.DTOs;
using FluentValidation;

namespace EcommerceMinified.Domain.Validators;

public class OrderValidator : AbstractValidator<OrderDto>
{
    public OrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order items are required")
            .When(x => x.Id == null);

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());
    }
}
