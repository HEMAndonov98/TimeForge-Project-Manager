using System.Linq.Expressions;

namespace TimeForge.Infrastructure.Repositories.Interfaces;

public interface IRepository : IDisposable
{
    IQueryable<T> All<T>() where T : class;

    IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class;

    Task<T?> GetByIdAsync<T>(object id) where T : class;

    Task<T> AddAsync<T>(T entity) where T : class;

    void Update<T>(T entity) where T : class;

    void Delete<T>(T entity) where T : class;

    Task SaveChangesAsync();
}