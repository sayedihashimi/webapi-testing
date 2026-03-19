using LibraryApi.DTOs.Author;
using LibraryApi.DTOs.Common;

namespace LibraryApi.Services.Interfaces;

public interface IAuthorService
{
    Task<PagedResult<AuthorListDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<AuthorDetailDto> GetByIdAsync(int id);
    Task<AuthorDetailDto> CreateAsync(CreateAuthorDto dto);
    Task<AuthorDetailDto> UpdateAsync(int id, UpdateAuthorDto dto);
    Task DeleteAsync(int id);
}
