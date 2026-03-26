using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class SkillService(HorizonDbContext db) : ISkillService
{
    public async Task<List<Skill>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Skills
            .AsNoTracking()
            .Include(s => s.EmployeeSkills)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Skill?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Skills
            .Include(s => s.EmployeeSkills).ThenInclude(es => es.Employee)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Skill> CreateAsync(string name, string category, string? description, CancellationToken ct = default)
    {
        var skill = new Skill
        {
            Name = name,
            Category = category,
            Description = description
        };

        db.Skills.Add(skill);
        await db.SaveChangesAsync(ct);
        return skill;
    }

    public async Task<Skill> UpdateAsync(int id, string name, string category, string? description, CancellationToken ct = default)
    {
        var skill = await db.Skills.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Skill {id} not found.");

        skill.Name = name;
        skill.Category = category;
        skill.Description = description;

        await db.SaveChangesAsync(ct);
        return skill;
    }

    public async Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId, CancellationToken ct = default)
    {
        return await db.EmployeeSkills
            .AsNoTracking()
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .OrderBy(es => es.Skill.Category)
            .ThenBy(es => es.Skill.Name)
            .ToListAsync(ct);
    }

    public async Task<EmployeeSkill> AddEmployeeSkillAsync(int employeeId, int skillId, ProficiencyLevel proficiency, int? yearsOfExperience, CancellationToken ct = default)
    {
        var exists = await db.EmployeeSkills.AnyAsync(es => es.EmployeeId == employeeId && es.SkillId == skillId, ct);
        if (exists)
            throw new InvalidOperationException("Employee already has this skill.");

        var es = new EmployeeSkill
        {
            EmployeeId = employeeId,
            SkillId = skillId,
            ProficiencyLevel = proficiency,
            YearsOfExperience = yearsOfExperience,
            LastAssessedDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        db.EmployeeSkills.Add(es);
        await db.SaveChangesAsync(ct);
        return es;
    }

    public async Task<EmployeeSkill> UpdateEmployeeSkillAsync(int id, ProficiencyLevel proficiency, int? yearsOfExperience, CancellationToken ct = default)
    {
        var es = await db.EmployeeSkills.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Employee skill {id} not found.");

        es.ProficiencyLevel = proficiency;
        es.YearsOfExperience = yearsOfExperience;
        es.LastAssessedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await db.SaveChangesAsync(ct);
        return es;
    }

    public async Task RemoveEmployeeSkillAsync(int id, CancellationToken ct = default)
    {
        var es = await db.EmployeeSkills.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Employee skill {id} not found.");

        db.EmployeeSkills.Remove(es);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minProficiency, CancellationToken ct = default)
    {
        var query = db.EmployeeSkills
            .AsNoTracking()
            .Include(es => es.Employee).ThenInclude(e => e.Department)
            .Where(es => es.SkillId == skillId);

        if (minProficiency.HasValue)
            query = query.Where(es => es.ProficiencyLevel >= minProficiency.Value);

        return await query
            .Select(es => es.Employee)
            .OrderBy(e => e.LastName)
            .ToListAsync(ct);
    }
}
