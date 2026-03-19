using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorSummaryDto>> GetAuthorsAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<AuthorDetailDto?> GetAuthorByIdAsync(int id, CancellationToken ct = default);
    Task<AuthorDetailDto> CreateAuthorAsync(CreateAuthorDto dto, CancellationToken ct = default);
    Task<AuthorDetailDto?> UpdateAuthorAsync(int id, UpdateAuthorDto dto, CancellationToken ct = default);
    Task<(bool Found, bool HasBooks)> DeleteAuthorAsync(int id, CancellationToken ct = default);
}
