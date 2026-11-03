using System.Reflection;
using AutoMapper;

namespace АСП3курс.Mapping;

public static class MappingExpressionExtensions
{
    public static IMappingExpression<TSource, TDestination> IgnoreNullMembers<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map)
    {
        var destinationProperties = typeof(TDestination).GetProperties();

        foreach (var sourceProperty in typeof(TSource).GetProperties())
        {
            if (!sourceProperty.CanRead)
            {
                continue;
            }

            if (!destinationProperties.Any(p => p.Name == sourceProperty.Name))
            {
                continue;
            }

            var property = sourceProperty;
            Func<TSource, bool> hasValue = source => property.GetValue(source) != null;

            map.ForMember(property.Name, options => options.PreCondition(hasValue));
        }

        return map;
    }
}
