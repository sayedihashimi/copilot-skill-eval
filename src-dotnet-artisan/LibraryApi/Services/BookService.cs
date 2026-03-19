using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PagedResult<BookResponse>> GetAllAsync(string? search, string? category, bool? available, string? sortBy, string? sortDir, int page, int pageSize);
    Task<BookDetailResponse?> GetByIdAsync(int id);
    Task<BookResponse> CreateAsync(CreateBookRequest request);
    Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<List<LoanResponse>> GetBookLoansAsync(int bookId);
    Task<List<ReservationResponse>> GetBookReservationsAsync(int bookId);
}

public class BookService(LibraryDbContext db) : IBookService
{
    public async Task<PagedResult<BookResponse>> GetAllAsync(
        string? search, string? category, bool? available, string? sortBy, string? sortDir, int page, int pageSize)
    {
        var query = db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                b.BookAuthors.Any(ba =>
                    ba.Author.FirstName.ToLower().Contains(term) ||
                    ba.Author.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var catTerm = category.Trim().ToLower();
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(catTerm)));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookResponse(b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Language, b.TotalCopies, b.AvailableCopies))
            .ToListAsync();

        return new PagedResult<BookResponse>(items, totalCount, page, pageSize);
    }

    public async Task<BookDetailResponse?> GetByIdAsync(int id)
    {
        return await db.Books
            .Where(b => b.Id == id)
            .Select(b => new BookDetailResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Description, b.PageCount,
                b.Language, b.TotalCopies, b.AvailableCopies, b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => new BookAuthorResponse(ba.Author.Id, ba.Author.FirstName, ba.Author.LastName)).ToList(),
                b.BookCategories.Select(bc => new BookCategoryResponse(bc.Category.Id, bc.Category.Name)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request)
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
        await db.SaveChangesAsync();

        if (request.AuthorIds is { Count: > 0 })
        {
            foreach (var authorId in request.AuthorIds)
                db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        if (request.CategoryIds is { Count: > 0 })
        {
            foreach (var categoryId in request.CategoryIds)
                db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync();

        return new BookResponse(book.Id, book.Title, book.ISBN, book.Publisher, book.PublicationYear, book.Language, book.TotalCopies, book.AvailableCopies);
    }

    public async Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";

        var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.TotalCopies - activeLoans;
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        db.BookAuthors.RemoveRange(book.BookAuthors);
        if (request.AuthorIds is { Count: > 0 })
        {
            foreach (var authorId in request.AuthorIds)
                db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        // Update categories
        db.BookCategories.RemoveRange(book.BookCategories);
        if (request.CategoryIds is { Count: > 0 })
        {
            foreach (var categoryId in request.CategoryIds)
                db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync();

        return new BookResponse(book.Id, book.Title, book.ISBN, book.Publisher, book.PublicationYear, book.Language, book.TotalCopies, book.AvailableCopies);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null) return (false, "Book not found.");

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans) return (false, "Cannot delete a book with active loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<LoanResponse>> GetBookLoansAsync(int bookId)
    {
        return await db.Loans
            .Where(l => l.BookId == bookId)
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();
    }

    public async Task<List<ReservationResponse>> GetBookReservationsAsync(int bookId)
    {
        return await db.Reservations
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync();
    }
}
