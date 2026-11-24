namespace be_rallyhubv2.contract.abstractions.pagination;

public sealed record PaginationRequest
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    private readonly int _page = 1;
    private readonly int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
    }
    
    public int Skip => (Page - 1) * PageSize;
}