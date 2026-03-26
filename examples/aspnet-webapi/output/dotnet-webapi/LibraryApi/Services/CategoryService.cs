using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext db, ILogger<CategoryService> logger)
    : ICategoryService
{
    public async Task<PaginatedResponse<CategoryResponse>> GetAllAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Categories.AsNoTracking().AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync(ct);

        return PaginatedResponse<CategoryResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<CategoryDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories
            .AsNoTracking()
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is null) return null;

        return new CategoryDetailResponse(
            category.Id, category.Name, category.Description,
            category.BookCategories.Count);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var exists = await db.Categories.AnyAsync(c => c.Name == request.Name, ct);
        if (exists)
            throw new InvalidOperationException($"A category with name '{request.Name}' already exists.");

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null) return null;

        var duplicate = await db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id, ct);
        if (duplicate)
            throw new InvalidOperationException($"A category with name '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated category {CategoryId}", category.Id);
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Count != 0)
            throw new InvalidOperationException($"Cannot delete category '{category.Name}' because it has associated books.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted category {CategoryId}", id);
    }
}
