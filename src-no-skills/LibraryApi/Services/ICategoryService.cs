using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page, int pageSize);
    Task<CategoryDetailDto> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto dto);
    Task<CategoryDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto);
    Task DeleteCategoryAsync(int id);
}
