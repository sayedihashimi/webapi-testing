using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public interface IClassTypeService
{
    Task<List<ClassTypeDto>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium);
    Task<ClassTypeDto> GetByIdAsync(int id);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto);
    Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto);
}
