using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HorizonHR.Services;

public sealed class SkillService : ISkillService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SkillService> _logger;

    public SkillService(ApplicationDbContext context, ILogger<SkillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
        return await _context.Skills
            .AsNoTracking()
            .Include(s => s.EmployeeSkills)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Skill?> GetSkillByIdAsync(int id)
    {
        return await _context.Skills
            .AsNoTracking()
            .Include(s => s.EmployeeSkills)
                .ThenInclude(es => es.Employee)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Skill> CreateSkillAsync(Skill skill)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created skill {SkillName} with ID {SkillId}", skill.Name, skill.Id);

        return skill;
    }

    public async Task<Skill> UpdateSkillAsync(Skill skill)
    {
        var existingSkill = await _context.Skills.FindAsync(skill.Id);
        if (existingSkill == null)
        {
            throw new InvalidOperationException($"Skill with ID {skill.Id} not found.");
        }

        _context.Entry(existingSkill).CurrentValues.SetValues(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated skill {SkillName} with ID {SkillId}", skill.Name, skill.Id);

        return skill;
    }

    public async Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId)
    {
        return await _context.EmployeeSkills
            .AsNoTracking()
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .OrderBy(es => es.Skill.Name)
            .ToListAsync();
    }

    public async Task<EmployeeSkill> AddEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added skill {SkillId} to employee {EmployeeId} at proficiency {Level}",
            employeeSkill.SkillId, employeeSkill.EmployeeId, employeeSkill.ProficiencyLevel);

        return employeeSkill;
    }

    public async Task<EmployeeSkill> UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        var existingEmployeeSkill = await _context.EmployeeSkills.FindAsync(employeeSkill.Id);
        if (existingEmployeeSkill == null)
        {
            throw new InvalidOperationException($"Employee skill with ID {employeeSkill.Id} not found.");
        }

        _context.Entry(existingEmployeeSkill).CurrentValues.SetValues(employeeSkill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated employee skill {EmployeeSkillId}", employeeSkill.Id);

        return employeeSkill;
    }

    public async Task RemoveEmployeeSkillAsync(int employeeSkillId)
    {
        var employeeSkill = await _context.EmployeeSkills.FindAsync(employeeSkillId);
        if (employeeSkill == null)
        {
            throw new InvalidOperationException($"Employee skill with ID {employeeSkillId} not found.");
        }

        _context.EmployeeSkills.Remove(employeeSkill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed employee skill {EmployeeSkillId}", employeeSkillId);
    }

    public async Task<List<Employee>> SearchEmployeesBySkillAsync(int skillId, ProficiencyLevel? minLevel)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.EmployeeSkills.Any(es =>
                es.SkillId == skillId &&
                (!minLevel.HasValue || es.ProficiencyLevel >= minLevel.Value)));

        return await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }
}
