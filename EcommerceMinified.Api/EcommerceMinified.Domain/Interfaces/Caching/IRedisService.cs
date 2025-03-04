using System;

namespace EcommerceMinified.Domain.Interfaces.Caching;

public interface IRedisService
{
    Task<T?> GetAsync<T>(Guid id);
    Task SetAsync<T>(Guid id, T value, TimeSpan? expiration = null);
    void Remove(string key);
}
