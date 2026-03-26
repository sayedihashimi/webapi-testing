using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class SkillService(ApplicationDbContext db) : ISkillService
{
    public async Task<List<Skill>> GetAllAsync()
    {
        return await db.Skills
            .Include(s => s.EmployeeSkills)
            .AsNoTracking()
            .OrderBy(s => s.Category).ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Skill?> GetByIdAsync(int id)
    {
        return await db.Skills
            .Include(s => s.EmployeeSkills)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Skill> CreateAsync(Skill skill)
    {
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        return skill;
    }

    public async Task UpdateAsync(Skill skill)
    {
        db.Skills.Update(skill);
        await db.SaveChangesAsync();
    }

    public async Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId)
    {
        return await db.EmployeeSkills
            .Include(es => es.Skill)
            .AsNoTracking()
            .Where(es => es.EmployeeId == employeeId)
            .OrderBy(es => es.Skill.Category).ThenBy(es => es.Skill.Name)
            .ToListAsync();
    }

    public async Task AddEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        var exists = await db.EmployeeSkills
            .AnyAsync(es => es.EmployeeId == employeeSkill.EmployeeId && es.SkillId == employeeSkill.SkillId);
        if (exists)
            throw new InvalidOperationException("Employee already has this skill.");

        db.EmployeeSkills.Add(employeeSkill);
        await db.SaveChangesAsync();
    }

    public async Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        db.EmployeeSkills.Update(employeeSkill);
        await db.SaveChangesAsync();
    }

    public async Task RemoveEmployeeSkillAsync(int employeeId, int skillId)
    {
        var es = await db.EmployeeSkills
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.SkillId == skillId);
        if (es is not null)
        {
            db.EmployeeSkills.Remove(es);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<EmployeeSkill>> SearchBySkillAsync(int skillId, ProficiencyLevel? minLevel = null)
    {
        var query = db.EmployeeSkills
            .Include(es => es.Employee).ThenInclude(e => e.Department)
            .Include(es => es.Skill)
            .AsNoTracking()
            .Where(es => es.SkillId == skillId);

        if (minLevel.HasValue)
            query = query.Where(es => es.ProficiencyLevel >= minLevel.Value);

        return await query
            .OrderByDescending(es => es.ProficiencyLevel)
            .ThenByDescending(es => es.YearsOfExperience)
            .ToListAsync();
    }
}
