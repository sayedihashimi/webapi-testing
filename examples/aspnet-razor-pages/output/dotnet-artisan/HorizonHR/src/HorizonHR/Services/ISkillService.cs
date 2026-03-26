using HorizonHR.Models;

namespace HorizonHR.Services;

public interface ISkillService
{
    Task<List<Skill>> GetAllAsync(CancellationToken ct = default);
    Task<Skill?> GetByIdAsync(int id, CancellationToken ct = default);
    Task CreateAsync(Skill skill, CancellationToken ct = default);
    Task UpdateAsync(Skill skill, CancellationToken ct = default);
    Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId, CancellationToken ct = default);
    Task AddEmployeeSkillAsync(EmployeeSkill employeeSkill, CancellationToken ct = default);
    Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill, CancellationToken ct = default);
    Task RemoveEmployeeSkillAsync(int employeeSkillId, CancellationToken ct = default);
    Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minProficiency = null, CancellationToken ct = default);
    Task<Dictionary<string, List<Skill>>> GetGroupedByCategoryAsync(CancellationToken ct = default);
}
