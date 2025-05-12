using System;
using EcommerceMinified.Domain.Interfaces.GrpcClients;
using FreightProtoService;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Polly.Registry;

namespace EcommerceMinified.Data.Grpc.Clients;

public class FreightClientService(ILogger<FreightClientService> logger, string grpcUrl, ResiliencePipelineProvider<string> _pipelineProvider) : BaseClientService<FreightService.FreightServiceClient>(logger, grpcUrl, _pipelineProvider), IFreightClientService<GetFreightDetailsRequest, GetFreightDetailsResponse>
{
    private readonly FreightProtoService.FreightService.FreightServiceClient _client;
    private readonly ILogger<FreightClientService> _logger;

    protected override FreightService.FreightServiceClient CreateClient(GrpcChannel channel)
    {
        return new FreightService.FreightServiceClient(channel);
    }

    public Task<GetFreightDetailsResponse> GetFreightInfoAsync(GetFreightDetailsRequest request)
    {
        return ExecuteGrpcCallAsync(
            request,
            req => Client.GetFreightAsyncAsync(req),
            nameof(GetFreightInfoAsync));
    }
}
