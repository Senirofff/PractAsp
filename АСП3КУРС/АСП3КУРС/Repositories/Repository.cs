using Microsoft.EntityFrameworkCore;
using АСП3курс.Models;

namespace АСП3курс.Repositories;

/// <inheritdoc />
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly OnlinePharmacyP824Context Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(OnlinePharmacyP824Context context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => DbSet.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(new object?[] { id }, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}
