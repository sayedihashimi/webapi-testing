namespace VetClinicApi.DTOs;

public record PaginationParams(int Page = 1, int PageSize = 10)
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize < 1 ? 10 : PageSize > 50 ? 50 : PageSize;
    public int Skip => (Page - 1) * PageSize;
}

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
