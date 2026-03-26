using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext context, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PagedResult<PatronResponse>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize)
    {
        var query = context.Patrons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(membershipType) && Enum.TryParse<MembershipType>(membershipType, true, out var mt))
        {
            query = query.Where(p => p.MembershipType == mt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync();

        return new PagedResult<PatronResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PatronDetailResponse?> GetPatronByIdAsync(int id)
    {
        var patron = await context.Patrons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (patron is null) return null;

        var activeLoans = await context.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0;

        return new PatronDetailResponse
        {
            Id = patron.Id,
            FirstName = patron.FirstName,
            LastName = patron.LastName,
            Email = patron.Email,
            Phone = patron.Phone,
            Address = patron.Address,
            MembershipDate = patron.MembershipDate,
            MembershipType = patron.MembershipType.ToString(),
            IsActive = patron.IsActive,
            CreatedAt = patron.CreatedAt,
            UpdatedAt = patron.UpdatedAt,
            ActiveLoansCount = activeLoans,
            TotalUnpaidFines = unpaidFines
        };
    }

    public async Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request)
    {
        if (await context.Patrons.AnyAsync(p => p.Email == request.Email))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipType = request.MembershipType,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Patrons.Add(patron);
        await context.SaveChangesAsync();
        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);
        return MapToResponse(patron);
    }

    public async Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request)
    {
        var patron = await context.Patrons.FindAsync(id);
        if (patron is null) return null;

        if (await context.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        logger.LogInformation("Updated patron {PatronId}", id);
        return MapToResponse(patron);
    }

    public async Task<bool> DeactivatePatronAsync(int id)
    {
        var patron = await context.Patrons.FindAsync(id);
        if (patron is null)
            throw new KeyNotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await context.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
            throw new InvalidOperationException("Cannot deactivate a patron with active loans. All loans must be returned first.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        logger.LogInformation("Deactivated patron {PatronId}", id);
        return true;
    }

    public async Task<PagedResult<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await context.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = context.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => BookService.MapLoanResponse(l))
            .ToListAsync();

        return new PagedResult<LoanResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        if (!await context.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = context.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => BookService.MapReservationResponse(r))
            .ToListAsync();

        return new PagedResult<ReservationResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await context.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = context.Fines
            .AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => FineService.MapFineResponse(f))
            .ToListAsync();

        return new PagedResult<FineResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static PatronResponse MapToResponse(Patron p) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address,
        MembershipDate = p.MembershipDate,
        MembershipType = p.MembershipType.ToString(),
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
