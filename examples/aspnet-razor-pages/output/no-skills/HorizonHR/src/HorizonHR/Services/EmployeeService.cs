using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Employee>> GetAllAsync(int pageNumber, int pageSize, string? search = null, int? departmentId = null, EmploymentType? employmentType = null, EmployeeStatus? status = null)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search) ||
                e.EmployeeNumber.ToLower().Contains(search));
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (employmentType.HasValue)
            query = query.Where(e => e.EmploymentType == employmentType.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        query = query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName);

        return await PaginatedList<Employee>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .Include(e => e.LeaveBalances).ThenInclude(lb => lb.LeaveType)
            .Include(e => e.PerformanceReviews).ThenInclude(pr => pr.Reviewer)
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.EmployeeNumber = await GenerateEmployeeNumberAsync();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Initialize leave balances for current year
        var currentYear = DateTime.UtcNow.Year;
        var leaveTypes = await _context.LeaveTypes.ToListAsync();
        foreach (var lt in leaveTypes)
        {
            _context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeaveTypeId = lt.Id,
                Year = currentYear,
                TotalDays = lt.DefaultDaysPerYear,
                UsedDays = 0,
                CarriedOverDays = 0
            });
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Employee created: {EmployeeName} ({EmployeeNumber})", employee.FullName, employee.EmployeeNumber);
        return employee;
    }

    public async Task UpdateAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task TerminateAsync(int employeeId, DateOnly terminationDate)
    {
        var employee = await _context.Employees
            .Include(e => e.LeaveRequests)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null) throw new InvalidOperationException("Employee not found.");

        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = terminationDate;
        employee.UpdatedAt = DateTime.UtcNow;

        // Cancel submitted leave requests
        foreach (var lr in employee.LeaveRequests.Where(r => r.Status == LeaveRequestStatus.Submitted))
        {
            lr.Status = LeaveRequestStatus.Cancelled;
            lr.UpdatedAt = DateTime.UtcNow;
        }

        // Remove as department manager
        var managedDepts = await _context.Departments
            .Where(d => d.ManagerId == employeeId)
            .ToListAsync();
        foreach (var dept in managedDepts)
        {
            dept.ManagerId = null;
            dept.UpdatedAt = DateTime.UtcNow;
        }

        // Remove as manager of direct reports
        var reports = await _context.Employees
            .Where(e => e.ManagerId == employeeId)
            .ToListAsync();
        foreach (var report in reports)
        {
            report.ManagerId = null;
            report.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Employee terminated: {EmployeeName} ({EmployeeNumber})", employee.FullName, employee.EmployeeNumber);
    }

    public async Task<string> GenerateEmployeeNumberAsync()
    {
        var maxNumber = await _context.Employees
            .Select(e => e.EmployeeNumber)
            .ToListAsync();

        int max = 0;
        foreach (var num in maxNumber)
        {
            if (num.StartsWith("EMP-") && int.TryParse(num[4..], out int n))
            {
                if (n > max) max = n;
            }
        }
        return $"EMP-{(max + 1):D4}";
    }

    public async Task<List<Employee>> GetByDepartmentAsync(int departmentId)
    {
        return await _context.Employees
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetDirectReportsAsync(int employeeId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.ManagerId == employeeId)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetAllActiveAsync()
    {
        return await _context.Employees
            .Where(e => e.Status != EmployeeStatus.Terminated)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Employees.Where(e => e.Status != EmployeeStatus.Terminated).CountAsync();
    }

    public async Task<List<Employee>> GetRecentHiresAsync(int days = 30)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        return await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.HireDate >= cutoff && e.Status != EmployeeStatus.Terminated)
            .OrderByDescending(e => e.HireDate)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetOnLeaveAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.Status == EmployeeStatus.OnLeave)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }
}
