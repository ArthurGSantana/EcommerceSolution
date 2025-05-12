using DnsClient.Internal;
using FreightProtoService;
using Grpc.Core;
using HubMinified.Domain.Enum;
using HubMinified.Domain.Interfaces.Repository;
using Microsoft.Extensions.Logging;

namespace HubMinified.Application.Grpc;

public class FreightGrpcService(IUnitOfWork _unitOfWork, ILogger<FreightGrpcService> _logger) : FreightProtoService.FreightService.FreightServiceBase
{

    public override async Task<GetFreightDetailsResponse> GetFreightAsync(GetFreightDetailsRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProductId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "O ID do produto é obrigatório"));
            }

            var product = await _unitOfWork.RepositoryProduct.Find(p => p.ProductId.ToString() == request.ProductId);

            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Produto não encontrado: {request.ProductId}"));
            }

            var baseFreightValue = product.ProductWeight * 0.5m;
            var random = new Random();

            var freightValue = baseFreightValue + random.Next(1, 100);
            var deliveryTime = random.Next(1, 10) + " dias";
            var deliveryType = ((DeliveryTypeEnum)random.Next(0, 2)).ToString();

            var response = new GetFreightDetailsResponse
            {
                FreightId = Guid.NewGuid().ToString(),
                FreightValue = (long)(freightValue * 100),
                DeliveryTime = deliveryTime,
                DeliveryType = deliveryType
            };

            return response;
        }
        catch (RpcException)
        {
            // Repassa exceções gRPC específicas
            throw;
        }
        catch (Exception ex)
        {
            // Log do erro
            _logger.LogError(ex, "Erro ao calcular o frete para o produto: {ProductId}", request.ProductId);
            throw new RpcException(new Status(StatusCode.Internal, "Erro ao calcular o frete"));
        }
    }
}
