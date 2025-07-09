namespace ProjectTemplate.Application.Common.QueryFilter;
public class PageList<T>
{
    public PageList(List<T> items, int pageIndex, int pageSize, int totalCount)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Total = totalCount;
        PageTotal = (int)Math.Ceiling(totalCount / (double)pageSize);
        Items = items;
    }
    public PageList(List<T> items, int pageIndex, int pageSize, int totalCount, int filteredTotal)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Total = totalCount;
        PageTotal = (int)Math.Ceiling(totalCount / (double)pageSize);
        Items = items;
        FilteredTotal = filteredTotal;
    }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int PageTotal { get; set; }
    public int FilteredTotal { get; set; }
    public List<T> Items { get; set; }
}
