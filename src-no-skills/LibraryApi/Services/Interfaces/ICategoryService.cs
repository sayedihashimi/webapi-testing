using LibraryApi.DTOs.Category;
using LibraryApi.DTOs.Common;

namespace LibraryApi.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryListDto>> GetAllAsync();
    Task<CategoryDetailDto> GetByIdAsync(int id);
    Task<CategoryDetailDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDetailDto> UpdateAsync(int id, UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}
