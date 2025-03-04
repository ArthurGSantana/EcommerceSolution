using System.Linq.Expressions;

namespace HubMinified.Domain.Interfaces.Mongo
{
    public interface IRepositoryBase<MODEL> where MODEL : class
    {
        Task<int> Save(MODEL model);
        Task Update(MODEL model, string id);
        Task Delete(string id);
        Task<List<MODEL>> Get(Expression<Func<MODEL, bool>> where, int skip, int take, Expression<Func<MODEL, object>> sort, bool asc = false);
        Task<List<MODEL>> Get(Expression<Func<MODEL, bool>> where);
        Task<long> Count(Expression<Func<MODEL, bool>> where);
        Task<MODEL> Find(Expression<Func<MODEL, bool>> where);
    }
}