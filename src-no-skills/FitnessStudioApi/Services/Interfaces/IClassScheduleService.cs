using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleResponseDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, PaginationParams pagination);
    Task<ClassScheduleResponseDto?> GetByIdAsync(int id);
    Task<ClassScheduleResponseDto> CreateAsync(ClassScheduleCreateDto dto);
    Task<ClassScheduleResponseDto?> UpdateAsync(int id, ClassScheduleUpdateDto dto);
    Task<ClassScheduleResponseDto> CancelAsync(int id, CancelClassDto dto);
    Task<List<RosterEntryDto>> GetRosterAsync(int id);
    Task<List<WaitlistEntryDto>> GetWaitlistAsync(int id);
    Task<List<ClassScheduleResponseDto>> GetAvailableAsync();
}
