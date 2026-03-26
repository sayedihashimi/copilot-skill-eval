using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Department>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .OrderBy(d => d.Name);

        return await PaginatedList<Department>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<List<Department>> GetHierarchyAsync()
    {
        return await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Include(d => d.ChildDepartments)
            .Where(d => d.ParentDepartmentId == null)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees).ThenInclude(e => e.Manager)
            .Include(d => d.ChildDepartments)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Department created: {DepartmentName} ({DepartmentCode})", department.Name, department.Code);
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Department updated: {DepartmentName}", department.Name);
    }

    public async Task<bool> HasCircularReference(int departmentId, int? parentId)
    {
        if (parentId == null) return false;
        if (parentId == departmentId) return true;

        var current = await _context.Departments.FindAsync(parentId);
        while (current != null)
        {
            if (current.ParentDepartmentId == departmentId) return true;
            if (current.ParentDepartmentId == null) break;
            current = await _context.Departments.FindAsync(current.ParentDepartmentId);
        }
        return false;
    }

    public async Task<List<Department>> GetAllFlatAsync()
    {
        return await _context.Departments.OrderBy(d => d.Name).ToListAsync();
    }
}
