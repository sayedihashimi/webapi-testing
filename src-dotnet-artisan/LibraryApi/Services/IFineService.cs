using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PaginatedResponse<FineResponse>> GetFinesAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<FineDetailResponse?> GetFineByIdAsync(int id, CancellationToken ct);
    Task<Result<FineDetailResponse>> PayFineAsync(int id, CancellationToken ct);
    Task<Result<FineDetailResponse>> WaiveFineAsync(int id, CancellationToken ct);
}
