using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PagedResult<FineDto>> GetFinesAsync(string? status, int page, int pageSize);
    Task<FineDto> GetFineByIdAsync(int id);
    Task<FineDto> PayFineAsync(int id);
    Task<FineDto> WaiveFineAsync(int id);
}
