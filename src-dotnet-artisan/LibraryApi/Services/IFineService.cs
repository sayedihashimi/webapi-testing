using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PagedResult<FineDto>> GetAllAsync(FineStatus? status, int page, int pageSize);
    Task<FineDto?> GetByIdAsync(int id);
    Task<FineDto> PayAsync(int id);
    Task<FineDto> WaiveAsync(int id);
}
