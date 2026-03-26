using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public class IndexModel : PageModel
{
    private readonly ISkillService _skillService;

    public IndexModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public Dictionary<string, List<Skill>> SkillsByCategory { get; set; } = new();

    public async Task OnGetAsync()
    {
        SkillsByCategory = await _skillService.GetGroupedByCategoryAsync();
    }
}
