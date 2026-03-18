using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PaginatedResponse<AuthorDto>> GetAllAsync(string? search, int page, int pageSize);
    Task<AuthorDetailDto?> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(CreateAuthorDto dto);
    Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}
