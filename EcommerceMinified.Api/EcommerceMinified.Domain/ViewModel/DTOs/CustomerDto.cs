using System;

namespace EcommerceMinified.Domain.ViewModel.DTOs;

public class CustomerDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Image { get; set; } = string.Empty;
    public AddressDto? Address { get; set; }
}
