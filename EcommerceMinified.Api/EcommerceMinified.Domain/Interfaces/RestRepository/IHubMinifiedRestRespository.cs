using System;
using EcommerceMinified.Domain.ViewModel.DTOs;

namespace EcommerceMinified.Domain.Interfaces.RestRepository;

public interface IHubMinifiedRestRespository
{
    Task<FreightResponseDto> GetFreightInfoAsync(FreightRequestDto freightRequest);
}
