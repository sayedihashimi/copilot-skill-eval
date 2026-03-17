using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db) : IBookService
{
    public async Task<PaginatedResponse<BookSummaryDto>> GetAllAsync(
        string? search, string? category, bool? available,
        string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                b.BookAuthors.Any(ba =>
                    ba.Author.FirstName.ToLower().Contains(term) ||
                    ba.Author.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var catTerm = category.Trim().ToLower();
            query = query.Where(b =>
                b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(catTerm)));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = sortBy?.ToLower() switch
        {
            "title" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "year" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(b => b.PublicationYear) : query.OrderBy(b => b.PublicationYear),
            "available" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(b => b.AvailableCopies) : query.OrderBy(b => b.AvailableCopies),
            _ => query.OrderBy(b => b.Title)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookSummaryDto(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
                b.Language, b.TotalCopies, b.AvailableCopies))
            .ToListAsync(ct);

        return new PaginatedResponse<BookSummaryDto>(items, total, page, pageSize);
    }

    public async Task<BookDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Books.AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookDetailDto(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Description,
                b.PageCount, b.Language, b.TotalCopies, b.AvailableCopies,
                b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => new AuthorDto(
                    ba.Author.Id, ba.Author.FirstName, ba.Author.LastName,
                    ba.Author.Biography, ba.Author.BirthDate, ba.Author.Country, ba.Author.CreatedAt)).ToList(),
                b.BookCategories.Select(bc => new CategoryDto(
                    bc.Category.Id, bc.Category.Name, bc.Category.Description)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<BookDetailDto> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Publisher = request.Publisher,
            PublicationYear = request.PublicationYear,
            Description = request.Description,
            PageCount = request.PageCount,
            Language = request.Language ?? "English",
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies
        };

        db.Books.Add(book);

        foreach (var authorId in request.AuthorIds)
            db.BookAuthors.Add(new BookAuthor { Book = book, AuthorId = authorId });

        foreach (var categoryId in request.CategoryIds)
            db.BookCategories.Add(new BookCategory { Book = book, CategoryId = categoryId });

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(book.Id, ct))!;
    }

    public async Task<BookDetailDto?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([id], ct);
        if (book is null) return null;

        var activeLoanCount = await db.Loans.CountAsync(l => l.BookId == id && l.Status != LoanStatus.Returned, ct);
        var newAvailable = request.TotalCopies - activeLoanCount;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, newAvailable);
        book.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(book.Id, ct))!;
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeleteAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([id], ct);
        if (book is null) return (false, false);

        var hasActive = await db.Loans.AnyAsync(l => l.BookId == id && l.Status != LoanStatus.Returned, ct);
        if (hasActive) return (true, true);

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
        return (true, false);
    }

    public async Task<PaginatedResponse<LoanDto>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking()
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(int bookId, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync(ct);
    }
}
