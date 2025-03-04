using System;
using HubMinified.Data.Mongo.Context;
using HubMinified.Domain.Interfaces.Mongo;
using HubMinified.Domain.Interfaces.Repository;
using MongoModel = HubMinified.Domain.MongoModels;
using MongoEntity = HubMinified.Data.Mongo.Entity;
using HubMinified.Data.Mongo.Repository;

namespace HubMinified.Data.Repository;

public class UnitOfWork(MongoContext _mongoContext) : IUnitOfWork
{
    private IRepositoryBase<MongoModel.Product>? _repositoryProduct;

    public IRepositoryBase<MongoModel.Product> RepositoryProduct => _repositoryProduct ?? (_repositoryProduct = new RepositoryBase<MongoModel.Product, MongoEntity.Product>(_mongoContext, "Product"));
}
