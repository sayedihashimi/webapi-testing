namespace VetClinicApi.DTOs;

public record PaginatedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
