using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<PerformanceReview>> GetAllAsync(int pageNumber, int pageSize, ReviewStatus? status = null, OverallRating? rating = null, int? departmentId = null)
    {
        var query = _context.PerformanceReviews
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (rating.HasValue)
            query = query.Where(r => r.OverallRating == rating.Value);

        if (departmentId.HasValue)
            query = query.Where(r => r.Employee.DepartmentId == departmentId.Value);

        query = query.OrderByDescending(r => r.ReviewPeriodEnd);

        return await PaginatedList<PerformanceReview>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<PerformanceReview?> GetByIdAsync(int id)
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PerformanceReview> CreateAsync(PerformanceReview review)
    {
        // Check for overlapping review periods
        var overlapping = await _context.PerformanceReviews
            .Where(r => r.EmployeeId == review.EmployeeId
                && r.ReviewPeriodStart <= review.ReviewPeriodEnd
                && r.ReviewPeriodEnd >= review.ReviewPeriodStart)
            .AnyAsync();

        if (overlapping)
            throw new InvalidOperationException("This employee already has a performance review with an overlapping review period.");

        review.Status = ReviewStatus.Draft;
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Performance review created for employee {EmployeeId} by reviewer {ReviewerId}", review.EmployeeId, review.ReviewerId);
        return review;
    }

    public async Task SubmitSelfAssessmentAsync(int reviewId, string selfAssessment)
    {
        var review = await _context.PerformanceReviews.FindAsync(reviewId);
        if (review == null) throw new InvalidOperationException("Review not found.");
        if (review.Status != ReviewStatus.SelfAssessmentPending)
            throw new InvalidOperationException("Review is not in the SelfAssessmentPending state.");

        review.SelfAssessment = selfAssessment;
        review.Status = ReviewStatus.ManagerReviewPending;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task CompleteManagerReviewAsync(int reviewId, string managerAssessment, OverallRating rating, string? strengths, string? improvements, string? goals)
    {
        var review = await _context.PerformanceReviews.FindAsync(reviewId);
        if (review == null) throw new InvalidOperationException("Review not found.");
        if (review.Status != ReviewStatus.ManagerReviewPending)
            throw new InvalidOperationException("Review is not in the ManagerReviewPending state.");

        review.ManagerAssessment = managerAssessment;
        review.OverallRating = rating;
        review.StrengthsNoted = strengths;
        review.AreasForImprovement = improvements;
        review.Goals = goals;
        review.Status = ReviewStatus.Completed;
        review.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Performance review {ReviewId} completed with rating {Rating}", reviewId, rating);
    }

    public async Task<int> GetUpcomingCountAsync()
    {
        return await _context.PerformanceReviews
            .CountAsync(r => r.Status != ReviewStatus.Completed);
    }
}
