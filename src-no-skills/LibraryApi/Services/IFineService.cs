using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PaginatedResponse<FineDto>> GetAllAsync(string? status, int page, int pageSize);
    Task<FineDto?> GetByIdAsync(int id);
    Task<(FineDto? Fine, string? Error)> PayAsync(int id);
    Task<(FineDto? Fine, string? Error)> WaiveAsync(int id);
}
