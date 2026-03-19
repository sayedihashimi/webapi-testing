using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<CategoryDetailDto?> GetCategoryByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<(bool Found, bool HasBooks)> DeleteCategoryAsync(int id, CancellationToken ct = default);
}
