

using HubMinified.Domain.Interfaces.Mongo;
using HubMinified.Domain.MongoModels;

namespace HubMinified.Domain.Interfaces.Repository
{
  public interface IUnitOfWork
  {
    IRepositoryBase<Product> RepositoryProduct { get; }
  }
}