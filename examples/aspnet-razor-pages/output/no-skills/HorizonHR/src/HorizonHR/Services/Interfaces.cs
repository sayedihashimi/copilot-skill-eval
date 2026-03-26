using HorizonHR.Models;

namespace HorizonHR.Services;

public interface IDepartmentService
{
    Task<PaginatedList<Department>> GetAllAsync(int pageNumber, int pageSize);
    Task<List<Department>> GetHierarchyAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task<bool> HasCircularReference(int departmentId, int? parentId);
    Task<List<Department>> GetAllFlatAsync();
}

public interface IEmployeeService
{
    Task<PaginatedList<Employee>> GetAllAsync(int pageNumber, int pageSize, string? search = null, int? departmentId = null, EmploymentType? employmentType = null, EmployeeStatus? status = null);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task TerminateAsync(int employeeId, DateOnly terminationDate);
    Task<string> GenerateEmployeeNumberAsync();
    Task<List<Employee>> GetByDepartmentAsync(int departmentId);
    Task<List<Employee>> GetDirectReportsAsync(int employeeId);
    Task<List<Employee>> GetAllActiveAsync();
    Task<int> GetTotalCountAsync();
    Task<List<Employee>> GetRecentHiresAsync(int days = 30);
    Task<List<Employee>> GetOnLeaveAsync();
}

public interface ILeaveService
{
    Task<PaginatedList<LeaveRequest>> GetAllRequestsAsync(int pageNumber, int pageSize, LeaveRequestStatus? status = null, int? employeeId = null, int? leaveTypeId = null);
    Task<LeaveRequest?> GetRequestByIdAsync(int id);
    Task<LeaveRequest> SubmitRequestAsync(LeaveRequest request);
    Task ApproveAsync(int requestId, int reviewerId, string? notes = null);
    Task RejectAsync(int requestId, int reviewerId, string? notes = null);
    Task CancelAsync(int requestId);
    Task<List<LeaveBalance>> GetBalancesAsync(int? employeeId = null, int? departmentId = null, int? year = null);
    Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int year);
    Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId);
    Task<List<LeaveType>> GetLeaveTypesAsync();
    Task<int> GetPendingCountAsync();
    Task<decimal> CalculateBusinessDays(DateOnly startDate, DateOnly endDate);
}

public interface IReviewService
{
    Task<PaginatedList<PerformanceReview>> GetAllAsync(int pageNumber, int pageSize, ReviewStatus? status = null, OverallRating? rating = null, int? departmentId = null);
    Task<PerformanceReview?> GetByIdAsync(int id);
    Task<PerformanceReview> CreateAsync(PerformanceReview review);
    Task SubmitSelfAssessmentAsync(int reviewId, string selfAssessment);
    Task CompleteManagerReviewAsync(int reviewId, string managerAssessment, OverallRating rating, string? strengths, string? improvements, string? goals);
    Task<int> GetUpcomingCountAsync();
}

public interface ISkillService
{
    Task<List<Skill>> GetAllAsync();
    Task<Skill?> GetByIdAsync(int id);
    Task<Skill> CreateAsync(Skill skill);
    Task UpdateAsync(Skill skill);
    Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId);
    Task AddEmployeeSkillAsync(EmployeeSkill employeeSkill);
    Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill);
    Task RemoveEmployeeSkillAsync(int employeeId, int skillId);
    Task<List<Employee>> SearchBySkillAsync(int skillId, ProficiencyLevel? minLevel = null);
    Task<Dictionary<string, List<Skill>>> GetGroupedByCategoryAsync();
}
