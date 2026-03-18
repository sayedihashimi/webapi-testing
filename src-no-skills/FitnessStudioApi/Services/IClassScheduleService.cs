using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleDto>> GetAllAsync(DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? available, int page, int pageSize);
    Task<ClassScheduleDto> GetByIdAsync(int id);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto);
    Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto);
    Task<List<ClassRosterItemDto>> GetRosterAsync(int id);
    Task<List<ClassRosterItemDto>> GetWaitlistAsync(int id);
    Task<List<ClassScheduleDto>> GetAvailableAsync();
}
