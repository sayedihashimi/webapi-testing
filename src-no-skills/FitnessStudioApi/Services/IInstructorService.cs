using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IInstructorService
{
    Task<List<InstructorDto>> GetAllAsync(string? specialization, bool? isActive);
    Task<InstructorDto> GetByIdAsync(int id);
    Task<InstructorDto> CreateAsync(CreateInstructorDto dto);
    Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto);
    Task<List<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to);
}
