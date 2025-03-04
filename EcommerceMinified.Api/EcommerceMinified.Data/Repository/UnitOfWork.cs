using System;
using Desafio4.Data.Postgres.Repository;
using EcommerceMinified.Data.Postgres.Context;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Interfaces.Postgres;
using EcommerceMinified.Domain.Interfaces.Repository;

namespace EcommerceMinified.Data.Repository;

public class UnitOfWork(PostgresDbContext _postgresContext) : IUnitOfWork
{
    #region Postgres
    private IRepositoryBase<Customer>? _customerRepository;

    private IRepositoryBase<Product>? _productRepository;

    private IRepositoryBase<Order>? _orderRepository;

    private IRepositoryBase<OrderItem>? _orderItemRepository;

    private IRepositoryBase<Address>? _addressRepository;

    public IRepositoryBase<Customer> CustomerRepository => _customerRepository ??= new RepositoryBase<Customer>(_postgresContext);
    public IRepositoryBase<Product> ProductRepository => _productRepository ??= new RepositoryBase<Product>(_postgresContext);
    public IRepositoryBase<Order> OrderRepository => _orderRepository ??= new RepositoryBase<Order>(_postgresContext);
    public IRepositoryBase<OrderItem> OrderItemRepository => _orderItemRepository ??= new RepositoryBase<OrderItem>(_postgresContext);
    public IRepositoryBase<Address> AddressRepository => _addressRepository ??= new RepositoryBase<Address>(_postgresContext);

    public async Task CommitPostgresAsync()
    {
        await _postgresContext.SaveChangesAsync();
    }
    #endregion
}
