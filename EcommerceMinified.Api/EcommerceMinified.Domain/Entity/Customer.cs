using System;

namespace EcommerceMinified.Domain.Entity;

public class Customer : Base
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Image { get; set; } = string.Empty;
    public Address? Address { get; set; }
}
