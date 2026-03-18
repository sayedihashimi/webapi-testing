namespace LibraryApi.DTOs;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public static PaginatedResponse<T> Create(List<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
