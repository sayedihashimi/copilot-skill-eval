using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Inspections;

public class IndexModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    public IndexModel(IInspectionService inspectionService) => _inspectionService = inspectionService;

    public PaginatedList<Inspection> Inspections { get; set; } = null!;
    [BindProperty(SupportsGet = true)] public InspectionType? Type { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Inspections = await _inspectionService.GetInspectionsAsync(Type, null, FromDate, ToDate, PageNumber, 10);
    }
}
