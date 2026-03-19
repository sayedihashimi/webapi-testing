using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassTypeService
{
    Task<List<ClassTypeResponseDto>> GetAllAsync(string? difficulty, bool? isPremium);
    Task<ClassTypeResponseDto?> GetByIdAsync(int id);
    Task<ClassTypeResponseDto> CreateAsync(ClassTypeCreateDto dto);
    Task<ClassTypeResponseDto?> UpdateAsync(int id, ClassTypeUpdateDto dto);
}
