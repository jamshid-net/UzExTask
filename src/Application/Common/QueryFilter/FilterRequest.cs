namespace ProjectTemplate.Application.Common.QueryFilter;
public class FilterRequest
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    private List<FilterSort>? _sort;
    public List<FilterSort> Sort
    {
        get => _sort ??= [];
        set => _sort = value;
    }

    private List<FilterObject>? _filter;
    public List<FilterObject> Filter
    {
        get => _filter ??= [];
        set => _filter = value;
    }
}

public record FilterSort : FilterBase<ZorroSortEnum>;
public record FilterObject : FilterBase<object>;
public abstract record FilterBase<T>
{
    public required string Key { get; set; }
    public required T Value { get; set; }
}

public enum ZorroSortEnum
{
    Asc = 0,
    Desc = 1,
}

