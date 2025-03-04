using System;
using HubMinified.Domain.ViewModel.DTOs;

namespace HubMinified.Domain.Interfaces.Services;

public interface IFreightService
{
     Task<FreightResponseDto> CalculateFreightAsync(FreightRequestDto freightRequest);
}
