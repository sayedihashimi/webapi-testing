using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PaginatedResponse<AuthorResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<AuthorDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken ct);
    Task<AuthorResponse?> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
