using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PaginatedResponse<FineResponse>> GetAllAsync(FineStatus? status, int page, int pageSize, CancellationToken ct);
    Task<FineResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<FineResponse> PayAsync(int id, CancellationToken ct);
    Task<FineResponse> WaiveAsync(int id, CancellationToken ct);
}
