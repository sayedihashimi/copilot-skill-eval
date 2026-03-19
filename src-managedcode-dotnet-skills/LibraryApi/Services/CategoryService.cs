using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class CategoryService(LibraryDbContext context, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Categories.CountAsync();
        var items = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync();

        return new PagedResult<CategoryDto>(items, totalCount, page, pageSize);
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(int id)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailDto(
                c.Id, c.Name, c.Description,
                c.BookCategories.Select(bc => new BookSummaryDto(
                    bc.Book.Id, bc.Book.Title, bc.Book.ISBN, bc.Book.TotalCopies, bc.Book.AvailableCopies
                )).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await context.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new InvalidOperationException($"Category with name '{dto.Name}' already exists.");

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null) return null;

        if (await context.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new InvalidOperationException($"Category with name '{dto.Name}' already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;

        await context.SaveChangesAsync();

        logger.LogInformation("Updated category {CategoryId}", id);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await context.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return false;

        if (category.BookCategories.Count > 0)
            throw new InvalidOperationException("Cannot delete category with associated books.");

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted category {CategoryId}", id);
        return true;
    }
}
