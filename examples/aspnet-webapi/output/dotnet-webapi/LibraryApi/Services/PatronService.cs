using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db, ILogger<PatronService> logger)
    : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetAllAsync(
        string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Patrons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);

        return PaginatedResponse<PatronResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (patron is null) return null;

        var activeLoansCount = await db.Loans
            .CountAsync(l => l.PatronId == id &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        var totalUnpaidFines = await db.Fines
            .Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        return new PatronDetailResponse(
            patron.Id, patron.FirstName, patron.LastName,
            patron.Email, patron.Phone, patron.Address,
            patron.MembershipDate, patron.MembershipType,
            patron.IsActive, patron.CreatedAt, patron.UpdatedAt,
            activeLoansCount, totalUnpaidFines);
    }

    public async Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct)
    {
        var emailExists = await db.Patrons.AnyAsync(p => p.Email == request.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MembershipType = request.MembershipType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}",
            patron.Id, patron.FirstName, patron.LastName);

        return MapToResponse(patron);
    }

    public async Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null) return null;

        var duplicateEmail = await db.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id, ct);
        if (duplicateEmail)
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated patron {PatronId}", patron.Id);
        return MapToResponse(patron);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons
            .Include(p => p.Loans)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        if (patron.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            throw new InvalidOperationException("Cannot deactivate patron because they have active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated patron {PatronId}", id);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(
        int patronId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var patronExists = await db.Patrons.AnyAsync(p => p.Id == patronId, ct);
        if (!patronExists)
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
            query = query.Where(l => l.Status == loanStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(LoanServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<LoanResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(
        int patronId, int page, int pageSize, CancellationToken ct)
    {
        var patronExists = await db.Patrons.AnyAsync(p => p.Id == patronId, ct);
        if (!patronExists)
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(ReservationServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<ReservationResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(
        int patronId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var patronExists = await db.Patrons.AnyAsync(p => p.Id == patronId, ct);
        if (!patronExists)
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Fines
            .AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fineStatus))
            query = query.Where(f => f.Status == fineStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(FineServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<FineResponse>.Create(responses, page, pageSize, totalCount);
    }

    private static PatronResponse MapToResponse(Patron p) =>
        new(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
            p.MembershipDate, p.MembershipType, p.IsActive, p.CreatedAt, p.UpdatedAt);
}
