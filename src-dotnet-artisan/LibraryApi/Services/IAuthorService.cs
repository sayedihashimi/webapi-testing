using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorDto>> GetAllAsync(string? search, int page, int pageSize);
    Task<AuthorDetailDto?> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(AuthorCreateDto dto);
    Task<AuthorDto?> UpdateAsync(int id, AuthorUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
