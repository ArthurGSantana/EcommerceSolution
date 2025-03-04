using System;
using EcommerceMinified.Domain.Interfaces.Commands;

namespace EcommerceMinified.Domain.Interfaces.Publishers;

public interface IProductInfoPublisherService
{
    Task PublishProductInfo(IProductInfoCommand productInfoCommand);
}
