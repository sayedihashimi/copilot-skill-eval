using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class AuthorService(LibraryDbContext context, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PagedResult<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize)
    {
        var query = context.Authors.AsNoTracking();

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
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return new PagedResult<AuthorResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AuthorDetailResponse?> GetAuthorByIdAsync(int id)
    {
        return await context.Authors
            .AsNoTracking()
            .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailResponse
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Biography = a.Biography,
                BirthDate = a.BirthDate,
                Country = a.Country,
                CreatedAt = a.CreatedAt,
                Books = a.BookAuthors.Select(ba => new AuthorBookResponse
                {
                    Id = ba.Book.Id,
                    Title = ba.Book.Title,
                    ISBN = ba.Book.ISBN,
                    PublicationYear = ba.Book.PublicationYear
                }).ToList()
            })
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
            Country = request.Country,
            CreatedAt = DateTime.UtcNow
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();
        logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);
        return MapToResponse(author);
    }

    public async Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request)
    {
        var author = await context.Authors.FindAsync(id);
        if (author is null) return null;

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;

        await context.SaveChangesAsync();
        logger.LogInformation("Updated author {AuthorId}", id);
        return MapToResponse(author);
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author is null)
            throw new KeyNotFoundException($"Author with ID {id} not found.");

        if (author.BookAuthors.Count != 0)
            throw new InvalidOperationException("Cannot delete an author that has books. Remove the books first.");

        context.Authors.Remove(author);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted author {AuthorId}", id);
        return true;
    }

    private static AuthorResponse MapToResponse(Author a) => new()
    {
        Id = a.Id,
        FirstName = a.FirstName,
        LastName = a.LastName,
        Biography = a.Biography,
        BirthDate = a.BirthDate,
        Country = a.Country,
        CreatedAt = a.CreatedAt
    };
}
