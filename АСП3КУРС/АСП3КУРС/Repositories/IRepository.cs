namespace АСП3курс.Repositories;

/// <summary>
/// Обобщённый репозиторий доступа к данным. Позволяет сервисам работать
/// с сущностями, не обращаясь напрямую к DbContext.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>Запрос для построения выборок и проекций (в том числе для AutoMapper ProjectTo).</summary>
    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет все отслеживаемые изменения. Так как все репозитории запроса
    /// используют один и тот же scoped DbContext, один этот вызов фиксирует
    /// изменения сразу нескольких сущностей (например, товара и резервирования).
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
