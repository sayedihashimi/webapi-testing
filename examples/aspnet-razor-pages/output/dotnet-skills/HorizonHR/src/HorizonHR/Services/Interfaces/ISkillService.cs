using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services.Interfaces;

public interface ISkillService
{
    Task<List<Skill>> GetSkillsAsync();
    Task<Skill?> GetSkillByIdAsync(int id);
    Task<Skill> CreateSkillAsync(Skill skill);
    Task<Skill> UpdateSkillAsync(Skill skill);
    Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId);
    Task<EmployeeSkill> AddEmployeeSkillAsync(EmployeeSkill employeeSkill);
    Task<EmployeeSkill> UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill);
    Task RemoveEmployeeSkillAsync(int employeeSkillId);
    Task<List<Employee>> SearchEmployeesBySkillAsync(int skillId, ProficiencyLevel? minLevel);
}
