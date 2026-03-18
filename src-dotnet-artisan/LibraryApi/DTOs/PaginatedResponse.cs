namespace LibraryApi.DTOs;

public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
