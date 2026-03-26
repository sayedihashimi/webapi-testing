using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class SkillService : ISkillService
{
    private readonly ApplicationDbContext _context;

    public SkillService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Skill>> GetAllAsync()
    {
        return await _context.Skills
            .Include(s => s.EmployeeSkills)
            .OrderBy(s => s.Category).ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Skill?> GetByIdAsync(int id)
    {
        return await _context.Skills
            .Include(s => s.EmployeeSkills).ThenInclude(es => es.Employee)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Skill> CreateAsync(Skill skill)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return skill;
    }

    public async Task UpdateAsync(Skill skill)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId)
    {
        return await _context.EmployeeSkills
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .OrderBy(es => es.Skill.Category).ThenBy(es => es.Skill.Name)
            .ToListAsync();
    }

    public async Task<EmployeeSkill> AddEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        var exists = await _context.EmployeeSkills
            .AnyAsync(es => es.EmployeeId == employeeSkill.EmployeeId && es.SkillId == employeeSkill.SkillId);

        if (exists)
            throw new InvalidOperationException("This employee already has this skill assigned.");

        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync();
        return employeeSkill;
    }

    public async Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        _context.EmployeeSkills.Update(employeeSkill);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveEmployeeSkillAsync(int id)
    {
        var skill = await _context.EmployeeSkills.FindAsync(id);
        if (skill != null)
        {
            _context.EmployeeSkills.Remove(skill);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EmployeeSkill?> GetEmployeeSkillByIdAsync(int id)
    {
        return await _context.EmployeeSkills
            .Include(es => es.Skill)
            .Include(es => es.Employee)
            .FirstOrDefaultAsync(es => es.Id == id);
    }

    public async Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minLevel = null)
    {
        var query = _context.EmployeeSkills
            .Include(es => es.Employee).ThenInclude(e => e.Department)
            .Include(es => es.Skill)
            .Where(es => es.SkillId == skillId);

        if (minLevel.HasValue)
            query = query.Where(es => es.ProficiencyLevel >= minLevel.Value);

        var results = await query.OrderByDescending(es => es.ProficiencyLevel).ToListAsync();
        return results.Select(es => es.Employee).ToList();
    }
}
