using System;
using EcommerceMinified.Domain.Enum;

namespace EcommerceMinified.Domain.ViewModel.DTOs;

public class ProductDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public ProductCategoryEnum Category { get; set; }
    public string? Image { get; set; } = string.Empty;
}
