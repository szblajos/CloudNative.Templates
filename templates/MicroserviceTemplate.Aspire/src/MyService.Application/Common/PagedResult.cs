namespace MyService.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T>? Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get { return (int)Math.Ceiling((double)TotalCount / PageSize); } }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // Parameterless constructor for JSON deserialization
    public PagedResult()
    {
        Items = [];
        TotalCount = 0;
        PageNumber = 1;
        PageSize = 10;
    }

    public PagedResult(IEnumerable<T>? items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
    public PagedResult(IEnumerable<T>? items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = 1;
        PageSize = totalCount;
    }
}