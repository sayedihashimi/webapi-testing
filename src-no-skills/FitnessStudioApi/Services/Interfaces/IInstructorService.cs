using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IInstructorService
{
    Task<List<InstructorResponseDto>> GetAllAsync(string? specialization, bool? isActive);
    Task<InstructorResponseDto?> GetByIdAsync(int id);
    Task<InstructorResponseDto> CreateAsync(InstructorCreateDto dto);
    Task<InstructorResponseDto?> UpdateAsync(int id, InstructorUpdateDto dto);
    Task<List<ClassScheduleResponseDto>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate);
}
