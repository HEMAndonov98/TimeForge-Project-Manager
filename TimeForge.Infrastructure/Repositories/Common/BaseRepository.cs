using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TimeForge.Infrastructure.Repositories.Interfaces;

namespace TimeForge.Infrastructure.Repositories.Common;

public abstract class BaseRepository<TContext>(TContext context) : IRepository
    where TContext : DbContext
{
    //TODO if repository breaks check this
    protected TContext Context { get; } = context;

    public IQueryable<T> All<T>() where T : class
        => this.Context.Set<T>();

    public IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class
        => this.Context.Set<T>().Where(search);

    public async Task<T?> GetByIdAsync<T>(object id) where T : class
        => await this.Context.Set<T>().FindAsync(id);

    public async Task<T> AddAsync<T>(T entity) where T : class
    {
        var entry = await this.Context.AddAsync(entity);
        return entry.Entity;
    }

    public void Update<T>(T entity) where T : class
    {
        this.Context.Set<T>().Update(entity);
    }

    //TODO Don't forget to add a query interceptor to modify command to safe delete entity when called
    public void Delete<T>(T entity) where T : class
        => this.Context.Set<T>().Remove(entity);

    public async Task SaveChangesAsync()
        => await this.Context.SaveChangesAsync();

    public void Dispose()
        => this.Context?.Dispose();
}