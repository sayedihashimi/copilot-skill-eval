using HorizonHR.Data;
using HorizonHR.Models;
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
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
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
            .OrderBy(es => es.Skill.Category)
            .ThenBy(es => es.Skill.Name)
            .ToListAsync();
    }

    public async Task AddEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        var exists = await _context.EmployeeSkills
            .AnyAsync(es => es.EmployeeId == employeeSkill.EmployeeId && es.SkillId == employeeSkill.SkillId);
        if (exists)
            throw new InvalidOperationException("Employee already has this skill.");

        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill)
    {
        _context.EmployeeSkills.Update(employeeSkill);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveEmployeeSkillAsync(int employeeId, int skillId)
    {
        var es = await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.SkillId == skillId);
        if (es != null)
        {
            _context.EmployeeSkills.Remove(es);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minLevel = null)
    {
        var query = _context.EmployeeSkills
            .Include(es => es.Employee).ThenInclude(e => e.Department)
            .Include(es => es.Skill)
            .Where(es => es.SkillId == skillId);

        if (minLevel.HasValue)
            query = query.Where(es => es.ProficiencyLevel >= minLevel.Value);

        var results = await query.ToListAsync();
        return results.Select(es => es.Employee).ToList();
    }

    public async Task<Dictionary<string, List<Skill>>> GetGroupedByCategoryAsync()
    {
        var skills = await _context.Skills
            .Include(s => s.EmployeeSkills)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return skills.GroupBy(s => s.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
