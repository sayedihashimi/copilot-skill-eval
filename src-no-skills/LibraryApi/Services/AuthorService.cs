using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<AuthorSummaryDto>> GetAuthorsAsync(string? search, int page, int pageSize)
    {
        var query = _db.Authors.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(s) || a.LastName.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AuthorSummaryDto { Id = a.Id, FirstName = a.FirstName, LastName = a.LastName })
            .ToListAsync();

        return new PagedResult<AuthorSummaryDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<AuthorDto?> GetAuthorByIdAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return null;

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Country = author.Country,
            CreatedAt = author.CreatedAt,
            Books = author.BookAuthors.Select(ba => new BookSummaryDto
            {
                Id = ba.Book.Id, Title = ba.Book.Title, ISBN = ba.Book.ISBN,
                AvailableCopies = ba.Book.AvailableCopies, TotalCopies = ba.Book.TotalCopies
            }).ToList()
        };
    }

    public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto dto)
    {
        var author = new Author
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Biography = dto.Biography,
            BirthDate = dto.BirthDate,
            Country = dto.Country,
            CreatedAt = DateTime.UtcNow
        };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Author created: {Id} {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);
        return (await GetAuthorByIdAsync(author.Id))!;
    }

    public async Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto dto)
    {
        var author = await _db.Authors.FindAsync(id);
        if (author == null) return null;

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;
        await _db.SaveChangesAsync();
        return (await GetAuthorByIdAsync(id))!;
    }

    public async Task<(bool Success, string? Error)> DeleteAuthorAsync(int id)
    {
        var author = await _db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return (false, "Author not found.");
        if (author.BookAuthors.Any()) return (false, "Cannot delete author that has books associated.");

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
