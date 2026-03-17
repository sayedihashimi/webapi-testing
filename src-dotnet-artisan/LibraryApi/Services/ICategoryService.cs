using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDetailDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
