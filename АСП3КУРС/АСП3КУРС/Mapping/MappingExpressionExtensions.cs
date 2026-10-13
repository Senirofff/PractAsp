using AutoMapper;

namespace АСП3курс.Mapping;

/// <summary>
/// Вспомогательные настройки профиля маппинга.
/// </summary>
public static class MappingExpressionExtensions
{
    /// <summary>
    /// Для PATCH-маппинга: не трогать свойство назначения, если соответствующее
    /// свойство DTO равно null (частичное обновление).
    /// </summary>
    public static IMappingExpression<TSource, TDestination> IgnoreNullMembers<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map)
    {
        // Делегат типизирован явно, чтобы компилятор выбрал перегрузку Condition
        // со значением члена (object), а не с ResolutionContext.
        Func<TSource, TDestination, object, bool> condition = (_, _, sourceMember) => sourceMember != null;

        map.ForAllMembers(memberOptions => memberOptions.Condition(condition));
        return map;
    }
}
