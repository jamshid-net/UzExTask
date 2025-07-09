using System.Linq.Expressions;
using System.Reflection;

namespace ProjectTemplate.Application.Common.QueryFilter;
public static class FilterQueryableExtension
{
    public static IQueryable<T> ApplyPageRequest<T>(this IQueryable<T> query, FilterRequest pageRequest)
    {
        var predicate = QueryFilterExpressionBuilder.BuildPredicate<T>(pageRequest);
        query = query.Where(predicate);

        // Apply sorting if required
        if (pageRequest.Sort.Any())
        {
            var sortedQuery = pageRequest.Sort.Aggregate(query, ApplySorting);
            query = sortedQuery;
        }

        // Apply paging
        query = query.Skip(pageRequest.PageIndex * pageRequest.PageSize)
                     .Take(pageRequest.PageSize);

        return query;
    }

    public static async Task<(IQueryable<T>, int)> ApplyPageRequestWithFilteredCount<T>(this IQueryable<T> query, FilterRequest pageRequest)
    {
        var predicate = QueryFilterExpressionBuilder.BuildPredicate<T>(pageRequest);
        query = query.Where(predicate);

        // Apply sorting if required
        if (pageRequest.Sort.Any())
        {
            var sortedQuery = pageRequest.Sort.Aggregate(query, ApplySorting);
            query = sortedQuery;
        }
        else
        {
            query = query.OrderBy(p => GetIdOrFirstProperty<T>());
        }
        int filteredCount = await query.CountAsync();
        // Apply paging
        query = query.Skip(pageRequest.PageIndex * pageRequest.PageSize)
                     .Take(pageRequest.PageSize);

        return (query, filteredCount);
    }

    private static IOrderedQueryable<T> ApplySorting<T>(IQueryable<T> query, FilterSort sort)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property;

        if (sort.Key.Contains(".multiLang.", StringComparison.OrdinalIgnoreCase))
        {
            // Split the key to get the actual property and the language code (e.g., "Name" and "uz")
            var keyParts = sort.Key.Split([".multiLang."], StringSplitOptions.None);
            var propertyName = keyParts[0];
            var languageCode = keyParts[1];

            // Get the main property (e.g., Name)
            var multiLanguageFieldProperty = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (multiLanguageFieldProperty == null)
            {
                throw new InvalidOperationException($"Property {propertyName} not found on type {typeof(T).Name}");
            }

            // Get the language-specific property within the MultiLanguageField (e.g., "uz")
            var multiLanguageFieldExpression = Expression.Property(parameter, multiLanguageFieldProperty);
            property = Expression.Property(multiLanguageFieldExpression, languageCode);
        }

        else
        {
            // Standard sorting for non-MultiLanguageField properties
            property = Expression.Property(parameter, sort.Key);
        }
        var lambda = Expression.Lambda(property, parameter);

        var methodName = sort.Value switch
        {
            ZorroSortEnum.Asc => "OrderBy",
            ZorroSortEnum.Desc => "OrderByDescending",
            _ => "OrderBy"
        };

        var method = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IOrderedQueryable<T>)method.Invoke(null, [query, lambda])!;
    }

    public static async Task<PageList<T>> ToPageListAsync<T>(
        this IQueryable<T> query,
        FilterRequest filterRequest,
        CancellationToken cancellationToken = default)
    {

        var count = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var (items, filteredCount) = await query.ApplyPageRequestWithFilteredCount(filterRequest);

        var result = await items.ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PageList<T>(result, filterRequest.PageIndex, filterRequest.PageSize, count, filteredCount);
    }


    private static string? GetIdOrFirstProperty<T>()
    {
        var type = typeof(T);

        var flags = BindingFlags.Public | BindingFlags.Instance;

        var idProperty = type.GetProperty("Id", flags);
        return !string.IsNullOrEmpty(idProperty?.Name) ?
                idProperty.Name :
                type.GetProperties(flags).FirstOrDefault()?.Name;
    }
}
