using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorResponse>> GetAllAsync(string? search, int page, int pageSize);
    Task<AuthorDetailResponse?> GetByIdAsync(int id);
    Task<AuthorResponse> CreateAsync(CreateAuthorRequest request);
    Task<AuthorResponse?> UpdateAsync(int id, UpdateAuthorRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class AuthorService(LibraryDbContext db) : IAuthorService
{
    public async Task<PagedResult<AuthorResponse>> GetAllAsync(string? search, int page, int pageSize)
    {
        var query = db.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.FirstName.ToLower().Contains(term) ||
                a.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync();

        return new PagedResult<AuthorResponse>(items, totalCount, page, pageSize);
    }

    public async Task<AuthorDetailResponse?> GetByIdAsync(int id)
    {
        return await db.Authors
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailResponse(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt,
                a.BookAuthors.Select(ba => new AuthorBookResponse(ba.Book.Id, ba.Book.Title, ba.Book.ISBN)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<AuthorResponse> CreateAsync(CreateAuthorRequest request)
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

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<AuthorResponse?> UpdateAsync(int id, UpdateAuthorRequest request)
    {
        var author = await db.Authors.FindAsync(id);
        if (author is null) return null;

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;

        await db.SaveChangesAsync();

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var author = await db.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author is null) return (false, "Author not found.");
        if (author.BookAuthors.Count > 0) return (false, "Cannot delete author with associated books.");

        db.Authors.Remove(author);
        await db.SaveChangesAsync();
        return (true, null);
    }
}
