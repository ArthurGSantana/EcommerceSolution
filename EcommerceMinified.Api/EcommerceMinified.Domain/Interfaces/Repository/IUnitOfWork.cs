using System;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Interfaces.Postgres;

namespace EcommerceMinified.Domain.Interfaces.Repository;

public interface IUnitOfWork
{
    IRepositoryBase<Customer> CustomerRepository { get; }
    IRepositoryBase<Product> ProductRepository { get; }
    IRepositoryBase<Order> OrderRepository { get; }
    IRepositoryBase<OrderItem> OrderItemRepository { get; }
    IRepositoryBase<Address> AddressRepository { get; }

    Task CommitPostgresAsync();
}
