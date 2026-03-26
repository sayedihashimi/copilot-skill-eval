using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public static class DataSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Departments.Any()) return;

        // Leave Types
        var vacation = new LeaveType { Id = 1, Name = "Vacation", DefaultDaysPerYear = 15, IsCarryOverAllowed = true, MaxCarryOverDays = 5, RequiresApproval = true, IsPaid = true };
        var sick = new LeaveType { Id = 2, Name = "Sick", DefaultDaysPerYear = 10, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var personal = new LeaveType { Id = 3, Name = "Personal", DefaultDaysPerYear = 3, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var bereavement = new LeaveType { Id = 4, Name = "Bereavement", DefaultDaysPerYear = 5, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = false, IsPaid = true };
        context.LeaveTypes.AddRange(vacation, sick, personal, bereavement);

        // Departments (without managers initially)
        var engineering = new Department { Id = 1, Name = "Engineering", Code = "ENG", Description = "Software engineering division", IsActive = true };
        var frontend = new Department { Id = 2, Name = "Frontend Engineering", Code = "FE", Description = "Frontend development team", ParentDepartmentId = 1, IsActive = true };
        var backend = new Department { Id = 3, Name = "Backend Engineering", Code = "BE", Description = "Backend development team", ParentDepartmentId = 1, IsActive = true };
        var hr = new Department { Id = 4, Name = "Human Resources", Code = "HR", Description = "Human resources department", IsActive = true };
        var marketing = new Department { Id = 5, Name = "Marketing", Code = "MKT", Description = "Marketing and communications", IsActive = true };
        context.Departments.AddRange(engineering, frontend, backend, hr, marketing);
        context.SaveChanges();

        // Employees
        var employees = new List<Employee>
        {
            new Employee { Id = 1, EmployeeNumber = "EMP-0001", FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@horizonhr.com", Phone = "555-0101", DateOfBirth = new DateOnly(1985, 3, 15), HireDate = new DateOnly(2020, 1, 10), DepartmentId = 1, JobTitle = "VP of Engineering", EmploymentType = EmploymentType.FullTime, Salary = 180000m, Status = EmployeeStatus.Active },
            new Employee { Id = 2, EmployeeNumber = "EMP-0002", FirstName = "Bob", LastName = "Smith", Email = "bob.smith@horizonhr.com", Phone = "555-0102", DateOfBirth = new DateOnly(1990, 7, 22), HireDate = new DateOnly(2021, 3, 15), DepartmentId = 2, JobTitle = "Frontend Team Lead", EmploymentType = EmploymentType.FullTime, Salary = 140000m, ManagerId = 1, Status = EmployeeStatus.Active },
            new Employee { Id = 3, EmployeeNumber = "EMP-0003", FirstName = "Carol", LastName = "Williams", Email = "carol.williams@horizonhr.com", Phone = "555-0103", DateOfBirth = new DateOnly(1992, 11, 5), HireDate = new DateOnly(2021, 6, 1), DepartmentId = 3, JobTitle = "Backend Team Lead", EmploymentType = EmploymentType.FullTime, Salary = 145000m, ManagerId = 1, Status = EmployeeStatus.Active },
            new Employee { Id = 4, EmployeeNumber = "EMP-0004", FirstName = "David", LastName = "Brown", Email = "david.brown@horizonhr.com", Phone = "555-0104", DateOfBirth = new DateOnly(1995, 1, 18), HireDate = new DateOnly(2022, 2, 14), DepartmentId = 2, JobTitle = "Senior Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 120000m, ManagerId = 2, Status = EmployeeStatus.Active },
            new Employee { Id = 5, EmployeeNumber = "EMP-0005", FirstName = "Eve", LastName = "Davis", Email = "eve.davis@horizonhr.com", Phone = "555-0105", DateOfBirth = new DateOnly(1998, 5, 30), HireDate = new DateOnly(2023, 1, 9), DepartmentId = 2, JobTitle = "Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 95000m, ManagerId = 2, Status = EmployeeStatus.Active },
            new Employee { Id = 6, EmployeeNumber = "EMP-0006", FirstName = "Frank", LastName = "Garcia", Email = "frank.garcia@horizonhr.com", Phone = "555-0106", DateOfBirth = new DateOnly(1988, 9, 12), HireDate = new DateOnly(2021, 8, 20), DepartmentId = 3, JobTitle = "Senior Backend Developer", EmploymentType = EmploymentType.FullTime, Salary = 130000m, ManagerId = 3, Status = EmployeeStatus.Active },
            new Employee { Id = 7, EmployeeNumber = "EMP-0007", FirstName = "Grace", LastName = "Martinez", Email = "grace.martinez@horizonhr.com", Phone = "555-0107", DateOfBirth = new DateOnly(1993, 4, 25), HireDate = new DateOnly(2022, 5, 16), DepartmentId = 3, JobTitle = "Backend Developer", EmploymentType = EmploymentType.Contract, Salary = 110000m, ManagerId = 3, Status = EmployeeStatus.Active },
            new Employee { Id = 8, EmployeeNumber = "EMP-0008", FirstName = "Henry", LastName = "Wilson", Email = "henry.wilson@horizonhr.com", Phone = "555-0108", DateOfBirth = new DateOnly(1987, 12, 8), HireDate = new DateOnly(2019, 11, 1), DepartmentId = 4, JobTitle = "HR Director", EmploymentType = EmploymentType.FullTime, Salary = 135000m, Status = EmployeeStatus.Active },
            new Employee { Id = 9, EmployeeNumber = "EMP-0009", FirstName = "Irene", LastName = "Taylor", Email = "irene.taylor@horizonhr.com", Phone = "555-0109", DateOfBirth = new DateOnly(1991, 6, 14), HireDate = new DateOnly(2022, 9, 5), DepartmentId = 4, JobTitle = "HR Specialist", EmploymentType = EmploymentType.FullTime, Salary = 75000m, ManagerId = 8, Status = EmployeeStatus.Active },
            new Employee { Id = 10, EmployeeNumber = "EMP-0010", FirstName = "Jack", LastName = "Anderson", Email = "jack.anderson@horizonhr.com", Phone = "555-0110", DateOfBirth = new DateOnly(1994, 8, 20), HireDate = new DateOnly(2023, 4, 17), DepartmentId = 5, JobTitle = "Marketing Manager", EmploymentType = EmploymentType.FullTime, Salary = 110000m, Status = EmployeeStatus.Active },
            new Employee { Id = 11, EmployeeNumber = "EMP-0011", FirstName = "Karen", LastName = "Thomas", Email = "karen.thomas@horizonhr.com", Phone = "555-0111", DateOfBirth = new DateOnly(1996, 2, 28), HireDate = new DateOnly(2024, 1, 8), DepartmentId = 5, JobTitle = "Marketing Coordinator", EmploymentType = EmploymentType.PartTime, Salary = 55000m, ManagerId = 10, Status = EmployeeStatus.Active },
            new Employee { Id = 12, EmployeeNumber = "EMP-0012", FirstName = "Leo", LastName = "Jackson", Email = "leo.jackson@horizonhr.com", Phone = "555-0112", DateOfBirth = new DateOnly(1989, 10, 3), HireDate = new DateOnly(2020, 7, 20), DepartmentId = 3, JobTitle = "DevOps Engineer", EmploymentType = EmploymentType.FullTime, Salary = 125000m, ManagerId = 3, Status = EmployeeStatus.Terminated, TerminationDate = new DateOnly(2025, 12, 31) },
            new Employee { Id = 13, EmployeeNumber = "EMP-0013", FirstName = "Maria", LastName = "Lopez", Email = "maria.lopez@horizonhr.com", Phone = "555-0113", DateOfBirth = new DateOnly(1997, 7, 11), HireDate = new DateOnly(2025, 6, 1), DepartmentId = 2, JobTitle = "Frontend Intern", EmploymentType = EmploymentType.Intern, Salary = 40000m, ManagerId = 2, Status = EmployeeStatus.OnLeave },
        };
        context.Employees.AddRange(employees);
        context.SaveChanges();

        // Set department managers
        engineering.ManagerId = 1; // Alice
        frontend.ManagerId = 2;   // Bob
        backend.ManagerId = 3;    // Carol
        hr.ManagerId = 8;         // Henry
        marketing.ManagerId = 10; // Jack
        context.SaveChanges();

        // Leave Balances for all active employees (current year)
        var currentYear = DateTime.UtcNow.Year;
        var leaveTypes = new[] { vacation, sick, personal, bereavement };
        var activeEmployees = employees.Where(e => e.Status != EmployeeStatus.Terminated).ToList();

        var balances = new List<LeaveBalance>();
        int balanceId = 1;
        foreach (var emp in activeEmployees)
        {
            foreach (var lt in leaveTypes)
            {
                var lb = new LeaveBalance
                {
                    Id = balanceId++,
                    EmployeeId = emp.Id,
                    LeaveTypeId = lt.Id,
                    Year = currentYear,
                    TotalDays = lt.DefaultDaysPerYear,
                    UsedDays = 0,
                    CarriedOverDays = 0
                };
                balances.Add(lb);
            }
        }
        // Add some used days
        var aliceVacation = balances.First(b => b.EmployeeId == 1 && b.LeaveTypeId == 1);
        aliceVacation.UsedDays = 5;
        var bobSick = balances.First(b => b.EmployeeId == 2 && b.LeaveTypeId == 2);
        bobSick.UsedDays = 2;
        var carolVacation = balances.First(b => b.EmployeeId == 3 && b.LeaveTypeId == 1);
        carolVacation.UsedDays = 3;
        var davidPersonal = balances.First(b => b.EmployeeId == 4 && b.LeaveTypeId == 3);
        davidPersonal.UsedDays = 1;

        context.LeaveBalances.AddRange(balances);
        context.SaveChanges();

        // Leave Requests
        var leaveRequests = new List<LeaveRequest>
        {
            new LeaveRequest { Id = 1, EmployeeId = 1, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 3, 10), EndDate = new DateOnly(currentYear, 3, 14), TotalDays = 5, Status = LeaveRequestStatus.Approved, Reason = "Spring vacation with family", ReviewedById = 8, ReviewDate = DateTime.UtcNow.AddDays(-30), SubmittedDate = DateTime.UtcNow.AddDays(-35) },
            new LeaveRequest { Id = 2, EmployeeId = 2, LeaveTypeId = 2, StartDate = new DateOnly(currentYear, 2, 5), EndDate = new DateOnly(currentYear, 2, 6), TotalDays = 2, Status = LeaveRequestStatus.Approved, Reason = "Flu recovery", ReviewedById = 1, ReviewDate = DateTime.UtcNow.AddDays(-50), SubmittedDate = DateTime.UtcNow.AddDays(-52) },
            new LeaveRequest { Id = 3, EmployeeId = 3, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 4, 21), EndDate = new DateOnly(currentYear, 4, 23), TotalDays = 3, Status = LeaveRequestStatus.Approved, Reason = "Personal travel", ReviewedById = 1, ReviewDate = DateTime.UtcNow.AddDays(-10), SubmittedDate = DateTime.UtcNow.AddDays(-15) },
            new LeaveRequest { Id = 4, EmployeeId = 4, LeaveTypeId = 3, StartDate = new DateOnly(currentYear, 5, 2), EndDate = new DateOnly(currentYear, 5, 2), TotalDays = 1, Status = LeaveRequestStatus.Approved, Reason = "Personal appointment", ReviewedById = 2, ReviewDate = DateTime.UtcNow.AddDays(-5), SubmittedDate = DateTime.UtcNow.AddDays(-7) },
            new LeaveRequest { Id = 5, EmployeeId = 5, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 6, 16), EndDate = new DateOnly(currentYear, 6, 20), TotalDays = 5, Status = LeaveRequestStatus.Submitted, Reason = "Summer vacation planned", SubmittedDate = DateTime.UtcNow.AddDays(-2) },
            new LeaveRequest { Id = 6, EmployeeId = 6, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 7, 7), EndDate = new DateOnly(currentYear, 7, 11), TotalDays = 5, Status = LeaveRequestStatus.Submitted, Reason = "Family reunion trip", SubmittedDate = DateTime.UtcNow.AddDays(-1) },
            new LeaveRequest { Id = 7, EmployeeId = 9, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 5, 12), EndDate = new DateOnly(currentYear, 5, 16), TotalDays = 5, Status = LeaveRequestStatus.Rejected, Reason = "Vacation abroad", ReviewedById = 8, ReviewDate = DateTime.UtcNow.AddDays(-8), ReviewNotes = "Team coverage insufficient during this period", SubmittedDate = DateTime.UtcNow.AddDays(-12) },
            new LeaveRequest { Id = 8, EmployeeId = 11, LeaveTypeId = 2, StartDate = new DateOnly(currentYear, 1, 20), EndDate = new DateOnly(currentYear, 1, 20), TotalDays = 1, Status = LeaveRequestStatus.Cancelled, Reason = "Doctor appointment", SubmittedDate = DateTime.UtcNow.AddDays(-60) },
        };
        context.LeaveRequests.AddRange(leaveRequests);
        context.SaveChanges();

        // Performance Reviews
        var reviews = new List<PerformanceReview>
        {
            new PerformanceReview { Id = 1, EmployeeId = 2, ReviewerId = 1, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.ExceedsExpectations, SelfAssessment = "I led the migration of our frontend to React 18 and mentored two junior developers.", ManagerAssessment = "Bob has consistently delivered high-quality work and shown strong leadership skills.", Goals = "Lead the design system initiative and explore micro-frontend architecture.", StrengthsNoted = "Technical leadership, mentoring, code quality", AreasForImprovement = "Cross-team communication could be improved", CompletedDate = new DateOnly(currentYear, 1, 15) },
            new PerformanceReview { Id = 2, EmployeeId = 3, ReviewerId = 1, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.MeetsExpectations, SelfAssessment = "Delivered the API modernization project on schedule and improved system reliability.", ManagerAssessment = "Carol met all objectives and maintained high service reliability.", Goals = "Lead the microservices migration and improve CI/CD pipeline.", StrengthsNoted = "System design, reliability engineering", AreasForImprovement = "Delegation of tasks to team members", CompletedDate = new DateOnly(currentYear, 1, 20) },
            new PerformanceReview { Id = 3, EmployeeId = 4, ReviewerId = 2, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.ManagerReviewPending, SelfAssessment = "Implemented the new dashboard components and improved page load time by 40%." },
            new PerformanceReview { Id = 4, EmployeeId = 5, ReviewerId = 2, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.SelfAssessmentPending },
            new PerformanceReview { Id = 5, EmployeeId = 6, ReviewerId = 3, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.Draft },
            new PerformanceReview { Id = 6, EmployeeId = 9, ReviewerId = 8, ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.Outstanding, SelfAssessment = "Streamlined the onboarding process reducing time-to-productivity by 30%.", ManagerAssessment = "Irene has been exceptional in improving HR processes and employee experience.", Goals = "Develop employee engagement program and implement HRIS improvements.", StrengthsNoted = "Process improvement, empathy, attention to detail", AreasForImprovement = "Public speaking and presentation skills", CompletedDate = new DateOnly(currentYear, 2, 1) },
        };
        context.PerformanceReviews.AddRange(reviews);
        context.SaveChanges();

        // Skills
        var skills = new List<Skill>
        {
            new Skill { Id = 1, Name = "Python", Category = "Programming Language", Description = "Python programming language" },
            new Skill { Id = 2, Name = "JavaScript", Category = "Programming Language", Description = "JavaScript/ECMAScript" },
            new Skill { Id = 3, Name = "C#", Category = "Programming Language", Description = "C# programming language" },
            new Skill { Id = 4, Name = "React", Category = "Framework", Description = "React frontend framework" },
            new Skill { Id = 5, Name = "ASP.NET Core", Category = "Framework", Description = "ASP.NET Core web framework" },
            new Skill { Id = 6, Name = "Docker", Category = "Tool", Description = "Container platform" },
            new Skill { Id = 7, Name = "SQL", Category = "Programming Language", Description = "Structured Query Language" },
            new Skill { Id = 8, Name = "Project Management", Category = "Soft Skill", Description = "Ability to plan and manage projects" },
            new Skill { Id = 9, Name = "Communication", Category = "Soft Skill", Description = "Effective verbal and written communication" },
            new Skill { Id = 10, Name = "Git", Category = "Tool", Description = "Version control system" },
            new Skill { Id = 11, Name = "TypeScript", Category = "Programming Language", Description = "TypeScript language" },
            new Skill { Id = 12, Name = "Kubernetes", Category = "Tool", Description = "Container orchestration platform" },
        };
        context.Skills.AddRange(skills);
        context.SaveChanges();

        // Employee Skills (20+ records)
        var employeeSkills = new List<EmployeeSkill>
        {
            new EmployeeSkill { Id = 1, EmployeeId = 1, SkillId = 3, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12, LastAssessedDate = new DateOnly(currentYear, 1, 15) },
            new EmployeeSkill { Id = 2, EmployeeId = 1, SkillId = 8, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10, LastAssessedDate = new DateOnly(currentYear, 1, 15) },
            new EmployeeSkill { Id = 3, EmployeeId = 1, SkillId = 9, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 12 },
            new EmployeeSkill { Id = 4, EmployeeId = 2, SkillId = 2, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8, LastAssessedDate = new DateOnly(currentYear, 2, 1) },
            new EmployeeSkill { Id = 5, EmployeeId = 2, SkillId = 4, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },
            new EmployeeSkill { Id = 6, EmployeeId = 2, SkillId = 11, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { Id = 7, EmployeeId = 3, SkillId = 3, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 7, LastAssessedDate = new DateOnly(currentYear, 1, 20) },
            new EmployeeSkill { Id = 8, EmployeeId = 3, SkillId = 5, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { Id = 9, EmployeeId = 3, SkillId = 7, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 7 },
            new EmployeeSkill { Id = 10, EmployeeId = 4, SkillId = 2, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { Id = 11, EmployeeId = 4, SkillId = 4, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new EmployeeSkill { Id = 12, EmployeeId = 5, SkillId = 2, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
            new EmployeeSkill { Id = 13, EmployeeId = 5, SkillId = 4, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 1 },
            new EmployeeSkill { Id = 14, EmployeeId = 6, SkillId = 3, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6, LastAssessedDate = new DateOnly(currentYear, 3, 1) },
            new EmployeeSkill { Id = 15, EmployeeId = 6, SkillId = 6, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new EmployeeSkill { Id = 16, EmployeeId = 7, SkillId = 1, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { Id = 17, EmployeeId = 7, SkillId = 7, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new EmployeeSkill { Id = 18, EmployeeId = 8, SkillId = 8, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15 },
            new EmployeeSkill { Id = 19, EmployeeId = 8, SkillId = 9, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15 },
            new EmployeeSkill { Id = 20, EmployeeId = 9, SkillId = 9, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new EmployeeSkill { Id = 21, EmployeeId = 10, SkillId = 8, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { Id = 22, EmployeeId = 10, SkillId = 9, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 },
            new EmployeeSkill { Id = 23, EmployeeId = 6, SkillId = 5, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { Id = 24, EmployeeId = 4, SkillId = 10, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 4 },
        };
        context.EmployeeSkills.AddRange(employeeSkills);
        context.SaveChanges();
    }
}
