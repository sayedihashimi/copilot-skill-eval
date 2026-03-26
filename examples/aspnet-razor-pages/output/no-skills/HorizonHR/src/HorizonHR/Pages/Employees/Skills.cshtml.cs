using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class SkillsModel : PageModel
{
    private readonly ISkillService _skillService;
    private readonly IEmployeeService _employeeService;

    public SkillsModel(ISkillService skillService, IEmployeeService employeeService)
    {
        _skillService = skillService;
        _employeeService = employeeService;
    }

    public Employee Employee { get; set; } = null!;
    public List<EmployeeSkill> EmployeeSkills { get; set; } = new();
    public List<SelectListItem> AvailableSkills { get; set; } = new();
    public List<SelectListItem> ProficiencyOptions { get; set; } = new();

    [BindProperty]
    public int SkillId { get; set; }

    [BindProperty]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    [BindProperty]
    public int? YearsOfExperience { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostAddAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        try
        {
            var employeeSkill = new EmployeeSkill
            {
                EmployeeId = id,
                SkillId = SkillId,
                ProficiencyLevel = ProficiencyLevel,
                YearsOfExperience = YearsOfExperience,
                LastAssessedDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            await _skillService.AddEmployeeSkillAsync(employeeSkill);
            TempData["SuccessMessage"] = "Skill added successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id, int skillId)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        await _skillService.RemoveEmployeeSkillAsync(id, skillId);
        TempData["SuccessMessage"] = "Skill removed successfully.";
        return RedirectToPage(new { id });
    }

    private async Task<IActionResult> LoadPageAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        Employee = employee;
        EmployeeSkills = await _skillService.GetEmployeeSkillsAsync(id);

        var allSkills = await _skillService.GetAllAsync();
        var assignedSkillIds = EmployeeSkills.Select(es => es.SkillId).ToHashSet();

        AvailableSkills = allSkills
            .Where(s => !assignedSkillIds.Contains(s.Id))
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} ({s.Category})"
            }).ToList();

        ProficiencyOptions = Enum.GetValues<ProficiencyLevel>()
            .Select(p => new SelectListItem
            {
                Value = ((int)p).ToString(),
                Text = p.ToString()
            }).ToList();

        return Page();
    }
}
