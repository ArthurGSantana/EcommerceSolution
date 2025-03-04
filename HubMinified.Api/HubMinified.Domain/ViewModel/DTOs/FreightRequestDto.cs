using System;

namespace HubMinified.Domain.ViewModel.DTOs;

public class FreightRequestDto
{
    public string ZipCode { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
}