using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService(LibraryDbContext context, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PagedResult<AuthorDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Authors.CountAsync();
        var items = await context.Authors
            .AsNoTracking()
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorDto(a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync();

        return new PagedResult<AuthorDto>(items, totalCount, page, pageSize);
    }

    public async Task<AuthorDetailDto?> GetByIdAsync(int id)
    {
        return await context.Authors
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailDto(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt,
                a.BookAuthors.Select(ba => new BookSummaryDto(
                    ba.Book.Id, ba.Book.Title, ba.Book.ISBN, ba.Book.TotalCopies, ba.Book.AvailableCopies
                )).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorDto dto)
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

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);

        return new AuthorDto(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorDto dto)
    {
        var author = await context.Authors.FindAsync(id);
        if (author is null) return null;

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;

        await context.SaveChangesAsync();

        logger.LogInformation("Updated author {AuthorId}", id);

        return new AuthorDto(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var author = await context.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id);
        if (author is null) return false;

        if (author.BookAuthors.Count > 0)
            throw new InvalidOperationException("Cannot delete author with associated books.");

        context.Authors.Remove(author);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted author {AuthorId}", id);
        return true;
    }
}
