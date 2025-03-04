using System;
using HubMinified.Domain.MongoModels;

namespace HubMinified.Domain.Interfaces.Services;

public interface IProductService
{
    Task CreateProductAsync(Product product);
}
