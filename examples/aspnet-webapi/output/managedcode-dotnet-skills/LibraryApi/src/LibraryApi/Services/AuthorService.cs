using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize)
    {
        var query = db.Authors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(term) || a.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<AuthorResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<AuthorResponse?> GetAuthorByIdAsync(int id)
    {
        return await db.Authors.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuthorResponse(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt,
                a.BookAuthors.Select(ba => new AuthorBookResponse(ba.Book.Id, ba.Book.Title, ba.Book.ISBN)).ToList()
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request)
    {
        var author = new Author
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Biography = request.Biography,
            BirthDate = request.BirthDate,
            Country = request.Country
        };

        db.Authors.Add(author);
        await db.SaveChangesAsync();

        logger.LogInformation("Author created: {AuthorId} {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request)
    {
        var author = await db.Authors.FindAsync(id);
        if (author is null) return null;

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;

        await db.SaveChangesAsync();

        logger.LogInformation("Author updated: {AuthorId}", id);

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id);
        if (author is null)
            throw new KeyNotFoundException($"Author with ID {id} not found.");

        if (author.BookAuthors.Count != 0)
            throw new InvalidOperationException("Cannot delete an author who has associated books.");

        db.Authors.Remove(author);
        await db.SaveChangesAsync();

        logger.LogInformation("Author deleted: {AuthorId}", id);
        return true;
    }
}
