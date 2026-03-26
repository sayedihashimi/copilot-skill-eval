using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class BalancesModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IDepartmentService _departmentService;

    public BalancesModel(ILeaveService leaveService, IDepartmentService departmentService)
    {
        _leaveService = leaveService;
        _departmentService = departmentService;
    }

    public List<LeaveBalance> Balances { get; set; } = new();
    public IEnumerable<IGrouping<int, LeaveBalance>> GroupedBalances { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    public SelectList DepartmentOptions { get; set; } = null!;
    public int CurrentYear { get; set; }

    public async Task OnGetAsync()
    {
        CurrentYear = DateTime.UtcNow.Year;

        var departments = await _departmentService.GetAllFlatAsync();
        DepartmentOptions = new SelectList(departments, "Id", "Name", DepartmentId);

        Balances = await _leaveService.GetBalancesAsync(departmentId: DepartmentId, year: CurrentYear);
        GroupedBalances = Balances.GroupBy(b => b.EmployeeId);
    }
}
