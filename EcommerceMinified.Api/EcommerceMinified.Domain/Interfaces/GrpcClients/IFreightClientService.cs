using System;

namespace EcommerceMinified.Domain.Interfaces.GrpcClients;

public interface IFreightClientService<TRequest, TResponse> where TRequest : class where TResponse : class
{
    Task<TResponse> GetFreightInfoAsync(TRequest freightRequest);
}
