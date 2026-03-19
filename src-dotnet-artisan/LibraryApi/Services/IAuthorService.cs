using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<AuthorDetailResponse?> GetAuthorByIdAsync(int id, CancellationToken ct);
    Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request, CancellationToken ct);
    Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request, CancellationToken ct);
    Task<(bool Found, bool HasBooks)> DeleteAuthorAsync(int id, CancellationToken ct);
}
