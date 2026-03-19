using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Authors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(term) || a.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<AuthorResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<AuthorDetailResponse?> GetAuthorByIdAsync(int id, CancellationToken ct)
    {
        return await db.Authors.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailResponse(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt,
                a.BookAuthors.Select(ba => new BookSummaryResponse(
                    ba.Book.Id, ba.Book.Title, ba.Book.ISBN, ba.Book.TotalCopies, ba.Book.AvailableCopies
                )).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request, CancellationToken ct)
    {
        var author = new Author
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Biography = request.Biography,
            BirthDate = request.BirthDate,
            Country = request.Country,
            CreatedAt = DateTime.UtcNow
        };

        db.Authors.Add(author);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Author created: {Id} - {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request, CancellationToken ct)
    {
        var author = await db.Authors.FindAsync([id], ct);
        if (author is null) return null;

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;

        await db.SaveChangesAsync(ct);

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteAuthorAsync(int id, CancellationToken ct)
    {
        var author = await db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (author is null) return (false, false);
        if (author.BookAuthors.Count > 0) return (true, true);

        db.Authors.Remove(author);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Author deleted: {Id}", id);
        return (true, false);
    }
}
