using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services;

public interface ISkillService
{
    Task<List<Skill>> GetAllAsync(CancellationToken ct = default);
    Task<Skill?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Skill> CreateAsync(string name, string category, string? description, CancellationToken ct = default);
    Task<Skill> UpdateAsync(int id, string name, string category, string? description, CancellationToken ct = default);
    Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId, CancellationToken ct = default);
    Task<EmployeeSkill> AddEmployeeSkillAsync(int employeeId, int skillId, ProficiencyLevel proficiency, int? yearsOfExperience, CancellationToken ct = default);
    Task<EmployeeSkill> UpdateEmployeeSkillAsync(int id, ProficiencyLevel proficiency, int? yearsOfExperience, CancellationToken ct = default);
    Task RemoveEmployeeSkillAsync(int id, CancellationToken ct = default);
    Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minProficiency, CancellationToken ct = default);
}
