using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PagedResult<FineDto>> GetFinesAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<FineDto?> GetFineByIdAsync(int id, CancellationToken ct = default);
    Task<(FineDto? Fine, string? Error)> PayFineAsync(int id, CancellationToken ct = default);
    Task<(FineDto? Fine, string? Error)> WaiveFineAsync(int id, CancellationToken ct = default);
}
