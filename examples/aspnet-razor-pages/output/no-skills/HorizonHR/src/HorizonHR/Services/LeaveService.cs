using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class LeaveService : ILeaveService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(ApplicationDbContext context, ILogger<LeaveService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<LeaveRequest>> GetAllRequestsAsync(int pageNumber, int pageSize, LeaveRequestStatus? status = null, int? employeeId = null, int? leaveTypeId = null)
    {
        var query = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(lr => lr.Status == status.Value);

        if (employeeId.HasValue)
            query = query.Where(lr => lr.EmployeeId == employeeId.Value);

        if (leaveTypeId.HasValue)
            query = query.Where(lr => lr.LeaveTypeId == leaveTypeId.Value);

        query = query.OrderByDescending(lr => lr.SubmittedDate);

        return await PaginatedList<LeaveRequest>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<LeaveRequest?> GetRequestByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<LeaveRequest> SubmitRequestAsync(LeaveRequest request)
    {
        // Check for overlapping leave
        var overlapping = await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == request.EmployeeId
                && (lr.Status == LeaveRequestStatus.Submitted || lr.Status == LeaveRequestStatus.Approved)
                && lr.StartDate <= request.EndDate
                && lr.EndDate >= request.StartDate)
            .AnyAsync();

        if (overlapping)
            throw new InvalidOperationException("This leave request overlaps with an existing submitted or approved leave request.");

        // Check leave balance
        var currentYear = request.StartDate.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                && lb.LeaveTypeId == request.LeaveTypeId
                && lb.Year == currentYear);

        if (balance == null)
            throw new InvalidOperationException("No leave balance found for this leave type and year.");

        if (balance.RemainingDays < request.TotalDays)
            throw new InvalidOperationException($"Insufficient leave balance. Available: {balance.RemainingDays} days, Requested: {request.TotalDays} days.");

        request.SubmittedDate = DateTime.UtcNow;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        request.Status = LeaveRequestStatus.Submitted;

        // Check if auto-approval applies
        var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeId);
        if (leaveType != null && !leaveType.RequiresApproval)
        {
            request.Status = LeaveRequestStatus.Approved;
            request.ReviewDate = DateTime.UtcNow;
            balance.UsedDays += request.TotalDays;
        }

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Leave request submitted for employee {EmployeeId}, type {LeaveTypeId}, {TotalDays} days", request.EmployeeId, request.LeaveTypeId, request.TotalDays);
        return request;
    }

    public async Task ApproveAsync(int requestId, int reviewerId, string? notes = null)
    {
        var request = await _context.LeaveRequests.FindAsync(requestId);
        if (request == null) throw new InvalidOperationException("Leave request not found.");
        if (request.Status != LeaveRequestStatus.Submitted) throw new InvalidOperationException("Only submitted requests can be approved.");

        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                && lb.LeaveTypeId == request.LeaveTypeId
                && lb.Year == request.StartDate.Year);

        if (balance != null)
        {
            if (balance.RemainingDays < request.TotalDays)
                throw new InvalidOperationException("Insufficient leave balance to approve this request.");
            balance.UsedDays += request.TotalDays;
        }

        request.Status = LeaveRequestStatus.Approved;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Leave request {RequestId} approved by {ReviewerId}", requestId, reviewerId);
    }

    public async Task RejectAsync(int requestId, int reviewerId, string? notes = null)
    {
        var request = await _context.LeaveRequests.FindAsync(requestId);
        if (request == null) throw new InvalidOperationException("Leave request not found.");
        if (request.Status != LeaveRequestStatus.Submitted) throw new InvalidOperationException("Only submitted requests can be rejected.");

        request.Status = LeaveRequestStatus.Rejected;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Leave request {RequestId} rejected by {ReviewerId}", requestId, reviewerId);
    }

    public async Task CancelAsync(int requestId)
    {
        var request = await _context.LeaveRequests.FindAsync(requestId);
        if (request == null) throw new InvalidOperationException("Leave request not found.");
        if (request.Status == LeaveRequestStatus.Cancelled) throw new InvalidOperationException("Request is already cancelled.");

        // Restore balance if was approved
        if (request.Status == LeaveRequestStatus.Approved)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                    && lb.LeaveTypeId == request.LeaveTypeId
                    && lb.Year == request.StartDate.Year);

            if (balance != null)
            {
                balance.UsedDays -= request.TotalDays;
                if (balance.UsedDays < 0) balance.UsedDays = 0;
            }
        }

        request.Status = LeaveRequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Leave request {RequestId} cancelled", requestId);
    }

    public async Task<List<LeaveBalance>> GetBalancesAsync(int? employeeId = null, int? departmentId = null, int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var query = _context.LeaveBalances
            .Include(lb => lb.Employee).ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.Year == targetYear)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(lb => lb.EmployeeId == employeeId.Value);

        if (departmentId.HasValue)
            query = query.Where(lb => lb.Employee.DepartmentId == departmentId.Value);

        return await query.OrderBy(lb => lb.Employee.LastName)
            .ThenBy(lb => lb.LeaveType.Name)
            .ToListAsync();
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int year)
    {
        return await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .OrderBy(lb => lb.LeaveType.Name)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<LeaveType>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes.OrderBy(lt => lt.Name).ToListAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Submitted);
    }

    public Task<decimal> CalculateBusinessDays(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate) return Task.FromResult(0m);

        decimal days = 0;
        var current = startDate;
        while (current <= endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                days++;
            current = current.AddDays(1);
        }
        return Task.FromResult(days);
    }
}
