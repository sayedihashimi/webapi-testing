using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class SkillService(ApplicationDbContext db) : ISkillService
{
    public async Task<List<Skill>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Skills
            .AsNoTracking()
            .Include(s => s.EmployeeSkills)
            .OrderBy(s => s.Category).ThenBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Skill?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Skills
            .Include(s => s.EmployeeSkills).ThenInclude(es => es.Employee)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task CreateAsync(Skill skill, CancellationToken ct = default)
    {
        db.Skills.Add(skill);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken ct = default)
    {
        db.Skills.Update(skill);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId, CancellationToken ct = default)
    {
        return await db.EmployeeSkills
            .AsNoTracking()
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .OrderBy(es => es.Skill.Name)
            .ToListAsync(ct);
    }

    public async Task AddEmployeeSkillAsync(EmployeeSkill employeeSkill, CancellationToken ct = default)
    {
        db.EmployeeSkills.Add(employeeSkill);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill, CancellationToken ct = default)
    {
        db.EmployeeSkills.Update(employeeSkill);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveEmployeeSkillAsync(int employeeSkillId, CancellationToken ct = default)
    {
        var es = await db.EmployeeSkills.FindAsync([employeeSkillId], ct);
        if (es is not null)
        {
            db.EmployeeSkills.Remove(es);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minProficiency = null,
        CancellationToken ct = default)
    {
        var query = db.EmployeeSkills
            .AsNoTracking()
            .Include(es => es.Employee).ThenInclude(e => e.Department)
            .Include(es => es.Skill)
            .Where(es => es.SkillId == skillId && es.Employee.Status != EmployeeStatus.Terminated);

        if (minProficiency.HasValue)
        {
            query = query.Where(es => es.ProficiencyLevel >= minProficiency.Value);
        }

        var results = await query.ToListAsync(ct);
        return results.Select(es => es.Employee).Distinct().ToList();
    }

    public async Task<Dictionary<string, List<Skill>>> GetGroupedByCategoryAsync(CancellationToken ct = default)
    {
        var skills = await db.Skills
            .AsNoTracking()
            .Include(s => s.EmployeeSkills)
            .OrderBy(s => s.Category).ThenBy(s => s.Name)
            .ToListAsync(ct);

        return skills.GroupBy(s => s.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
