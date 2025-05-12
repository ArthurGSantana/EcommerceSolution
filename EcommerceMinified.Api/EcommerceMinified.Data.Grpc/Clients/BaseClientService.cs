using System;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace EcommerceMinified.Data.Grpc.Clients;

public abstract class BaseClientService<TClient> where TClient : class
{
    private readonly ILogger _logger;
    private readonly string _grpcUrl;
    protected readonly TClient Client;

    protected BaseClientService(ILogger logger, string grpcUrl)
    {
        _logger = logger;

        var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                EnableMultipleHttp2Connections = true,
            }
        });

        Client = CreateClient(channel);
    }

    protected abstract TClient CreateClient(GrpcChannel channel);

    protected async Task<TResponse> ExecuteGrpcCallAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, AsyncUnaryCall<TResponse>> grpcMethod,
        string methodName)
    {
        try
        {
            _logger.LogInformation("Calling gRPC method {MethodName}", methodName);
            var response = await grpcMethod(request);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error in {MethodName}. Status: {Status}, Detail: {Detail}",
                methodName, ex.StatusCode, ex.Status.Detail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in gRPC call {MethodName}", methodName);
            throw;
        }
    }
}
