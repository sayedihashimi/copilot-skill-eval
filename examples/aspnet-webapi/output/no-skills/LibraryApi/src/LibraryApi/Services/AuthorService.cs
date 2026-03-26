using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;

    public AuthorService(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<AuthorDto>> GetAllAsync(string? search, int page, int pageSize)
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
            .Select(a => MapToDto(a)).ToListAsync();

        return new PagedResult<AuthorDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<AuthorDetailDto?> GetByIdAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) return null;

        return new AuthorDetailDto
        {
            Id = author.Id, FirstName = author.FirstName, LastName = author.LastName,
            Biography = author.Biography, BirthDate = author.BirthDate, Country = author.Country,
            CreatedAt = author.CreatedAt,
            Books = author.BookAuthors.Select(ba => new BookSummaryDto
            {
                Id = ba.Book.Id, Title = ba.Book.Title, ISBN = ba.Book.ISBN,
                TotalCopies = ba.Book.TotalCopies, AvailableCopies = ba.Book.AvailableCopies
            }).ToList()
        };
    }

    public async Task<AuthorDto> CreateAsync(AuthorCreateDto dto)
    {
        var author = new Author
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Biography = dto.Biography,
            BirthDate = dto.BirthDate, Country = dto.Country, CreatedAt = DateTime.UtcNow
        };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        return MapToDto(author);
    }

    public async Task<AuthorDto?> UpdateAsync(int id, AuthorUpdateDto dto)
    {
        var author = await _db.Authors.FindAsync(id);
        if (author == null) return null;

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;
        await _db.SaveChangesAsync();
        return MapToDto(author);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var author = await _db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null) throw new NotFoundException($"Author with ID {id} not found.");
        if (author.BookAuthors.Any()) throw new BusinessRuleException("Cannot delete an author who has books. Remove the book associations first.");

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
        return true;
    }

    private static AuthorDto MapToDto(Author a) => new()
    {
        Id = a.Id, FirstName = a.FirstName, LastName = a.LastName,
        Biography = a.Biography, BirthDate = a.BirthDate, Country = a.Country, CreatedAt = a.CreatedAt
    };
}
