using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;

    public IndexModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public DashboardStats Stats { get; set; } = new();

    public async Task OnGetAsync()
    {
        Stats = await _dashboardService.GetStatsAsync();
    }
}
