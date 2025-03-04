using System;
using HubMinified.Domain.Interfaces.Repository;
using HubMinified.Domain.Interfaces.Services;
using HubMinified.Domain.MongoModels;

namespace HubMinified.Application.Services;

public class ProductService(IUnitOfWork _unitOfWork) : IProductService
{
    public async Task CreateProductAsync(Product product)
    {
        await _unitOfWork.RepositoryProduct.Save(product);
    }
}
