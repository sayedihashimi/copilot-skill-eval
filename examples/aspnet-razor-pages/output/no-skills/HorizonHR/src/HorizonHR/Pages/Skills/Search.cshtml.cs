using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Skills;

public class SearchModel : PageModel
{
    private readonly ISkillService _skillService;

    public SearchModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [BindProperty(SupportsGet = true)]
    public int? SkillId { get; set; }

    [BindProperty(SupportsGet = true)]
    public ProficiencyLevel? MinLevel { get; set; }

    public List<SelectListItem> SkillOptions { get; set; } = new();
    public List<SelectListItem> ProficiencyOptions { get; set; } = new();
    public List<Employee> Results { get; set; } = new();
    public bool SearchPerformed { get; set; }

    public async Task OnGetAsync()
    {
        await LoadSkillOptionsAsync();

        if (SkillId.HasValue)
        {
            SearchPerformed = true;
            Results = await _skillService.SearchBySkillAsync(SkillId.Value, MinLevel);
        }
    }

    private async Task LoadSkillOptionsAsync()
    {
        var skills = await _skillService.GetAllAsync();
        SkillOptions = skills.Select(s => new SelectListItem
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
    }
}
