using System;
using HubMinified.Domain.Enum;
using HubMinified.Domain.Interfaces.Repository;
using HubMinified.Domain.Interfaces.Services;
using HubMinified.Domain.ViewModel.DTOs;

namespace HubMinified.Application.Services;

public class FreightService(IUnitOfWork _unitOfWork) : IFreightService
{
        public async Task<FreightResponseDto> CalculateFreightAsync(FreightRequestDto freightRequest)
    {
        var product = await _unitOfWork.RepositoryProduct.Find(p => p.ProductId.ToString() == freightRequest.ProductId.ToString());
        var baseFreightValue = (product?.ProductWeight ?? 20) * 0.5m;
    
        var random = new Random();
    
        var freightValue = baseFreightValue + random.Next(1, 100);
        var deliveryTime = random.Next(1, 10) + " dias";
        var deliveryType = ((DeliveryTypeEnum)random.Next(0, 2)).ToString();
    
        return new FreightResponseDto
        {
            FreightValue = freightValue,
            DeliveryTime = deliveryTime,
            DeliveryType = deliveryType
        };
    }
}
