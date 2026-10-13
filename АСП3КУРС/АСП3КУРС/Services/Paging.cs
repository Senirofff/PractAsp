namespace АСП3курс.Services;

/// <summary>Нормализация параметров пагинации, общая для всех сервисов.</summary>
internal static class Paging
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        return (page, pageSize);
    }
}
