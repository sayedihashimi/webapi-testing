using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorDto>> GetAuthorsAsync(string? search, int page, int pageSize);
    Task<AuthorDetailDto> GetAuthorByIdAsync(int id);
    Task<AuthorDto> CreateAuthorAsync(AuthorCreateDto dto);
    Task<AuthorDto> UpdateAuthorAsync(int id, AuthorUpdateDto dto);
    Task DeleteAuthorAsync(int id);
}
