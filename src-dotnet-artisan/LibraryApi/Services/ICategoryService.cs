using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(int page, int pageSize, CancellationToken ct);
    Task<CategoryDetailResponse?> GetCategoryByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct);
    Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken ct);
    Task<(bool Found, bool HasBooks)> DeleteCategoryAsync(int id, CancellationToken ct);
}
