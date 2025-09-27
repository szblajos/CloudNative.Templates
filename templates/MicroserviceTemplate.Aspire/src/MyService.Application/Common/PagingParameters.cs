namespace MyService.Application.Common;

public class PagingParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public PagingParameters() { }
    public PagingParameters(int? pageNumber, int? pageSize)
    {
        if (pageNumber.HasValue && pageNumber > 0)
            PageNumber = pageNumber.Value;

        if (pageSize.HasValue && pageSize > 0)
            PageSize = pageSize.Value;
    }
}