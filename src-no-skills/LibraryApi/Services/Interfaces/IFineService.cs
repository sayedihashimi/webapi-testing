using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Fine;
using LibraryApi.Models.Enums;

namespace LibraryApi.Services.Interfaces;

public interface IFineService
{
    Task<PagedResult<FineListDto>> GetAllAsync(FineStatus? status, PaginationParams pagination);
    Task<FineDetailDto> GetByIdAsync(int id);
    Task<FineDetailDto> PayAsync(int id);
    Task<FineDetailDto> WaiveAsync(int id);
}
