namespace АСП3курс.Services;

/// <summary>Результат постраничной выборки: элементы страницы и общее количество записей.</summary>
public class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int TotalCount { get; }
}
