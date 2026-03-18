using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryDto>> GetAllAsync(int page, int pageSize);
    Task<CategoryDetailDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}
