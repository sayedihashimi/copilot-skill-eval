using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService(LibraryDbContext context, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PagedResult<PatronDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Patrons.CountAsync();
        var items = await context.Patrons
            .AsNoTracking()
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResult<PatronDto>(items, totalCount, page, pageSize);
    }

    public async Task<PatronDto?> GetByIdAsync(int id)
    {
        var patron = await context.Patrons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return patron is null ? null : MapToDto(patron);
    }

    public async Task<PatronDto> CreateAsync(CreatePatronDto dto)
    {
        if (await context.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new InvalidOperationException($"A patron with email '{dto.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MembershipType = dto.MembershipType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Patrons.Add(patron);
        await context.SaveChangesAsync();

        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return MapToDto(patron);
    }

    public async Task<PatronDto?> UpdateAsync(int id, UpdatePatronDto dto)
    {
        var patron = await context.Patrons.FindAsync(id);
        if (patron is null) return null;

        if (await context.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new InvalidOperationException($"A patron with email '{dto.Email}' already exists.");

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.IsActive = dto.IsActive;
        patron.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Updated patron {PatronId}", id);

        return MapToDto(patron);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patron = await context.Patrons
            .Include(p => p.Loans.Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patron is null) return false;

        if (patron.Loans.Count > 0)
            throw new InvalidOperationException("Cannot delete patron with active or overdue loans.");

        context.Patrons.Remove(patron);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted patron {PatronId}", id);
        return true;
    }

    public async Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, int page, int pageSize)
    {
        if (!await context.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = context.Loans.AsNoTracking().Where(l => l.PatronId == patronId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PagedResult<LoanDto>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        if (!await context.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = context.Reservations.AsNoTracking().Where(r => r.PatronId == patronId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync();

        return new PagedResult<ReservationDto>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, int page, int pageSize)
    {
        if (!await context.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = context.Fines.AsNoTracking().Where(f => f.PatronId == patronId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineDto(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync();

        return new PagedResult<FineDto>(items, totalCount, page, pageSize);
    }

    private static PatronDto MapToDto(Patron p) => new(
        p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
        p.MembershipDate, p.MembershipType, p.IsActive, p.CreatedAt, p.UpdatedAt);
}
