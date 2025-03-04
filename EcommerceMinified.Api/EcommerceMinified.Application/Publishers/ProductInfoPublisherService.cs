using System;
using EcommerceMinified.Domain.Interfaces.Commands;
using EcommerceMinified.Domain.Interfaces.Publishers;
using EcommerceMinified.MsgContracts.Command;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly.Registry;

namespace EcommerceMinified.Application.Publishers;

public class ProductInfoPublisherService(ILogger<ProductInfoPublisherService> _logger, IPublishEndpoint _publishEndpoint, ResiliencePipelineProvider<string> _pipelineProvider) : IProductInfoPublisherService
{
    private readonly string _resilienceKey = "default";

    public async Task PublishProductInfo(IProductInfoCommand productInfoCommand)
    {
        var pipeline = _pipelineProvider.GetPipeline(_resilienceKey);

        try
        {
            await pipeline.ExecuteAsync(async (_) =>
            {
                await _publishEndpoint.Publish(productInfoCommand);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while publishing product info");
        }
    }
}
