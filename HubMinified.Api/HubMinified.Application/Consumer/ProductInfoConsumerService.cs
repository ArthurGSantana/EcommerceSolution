using System;
using DnsClient.Internal;
using EcommerceMinified.Domain.Interfaces.Commands;
using HubMinified.Domain.Interfaces.Services;
using HubMinified.Domain.MongoModels;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HubMinified.Application.Consumer;

public class ProductInfoConsumerService(ILogger<ProductInfoConsumerService> _logger, IProductService _productService) : IConsumer<IProductInfoCommand>
{
    public async Task Consume(ConsumeContext<IProductInfoCommand> context)
    {
        try
        {
            var product = new Product
            {
                ProductId = context.Message.ProductId,
                ProductName = context.Message.ProductName,
                ProductWeight = context.Message.ProductWeight
            };

            await _productService.CreateProductAsync(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProductInfoConsumerService");
        }
    }
}
