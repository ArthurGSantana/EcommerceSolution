using System;

namespace EcommerceMinified.Domain.ViewModel.DTOs;

public class FreightResponseDto
{
    public decimal FreightValue { get; set; }
    public string DeliveryTime { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
}
