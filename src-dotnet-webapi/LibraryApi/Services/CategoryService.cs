using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext db, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await db.Categories.CountAsync(ct);
        var items = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync(ct);

        return new PaginatedResponse<CategoryResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<CategoryDetailResponse?> GetCategoryByIdAsync(int id, CancellationToken ct)
    {
        return await db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailResponse(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Category created: {Id} - {Name}", category.Id, category.Name);

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null) return null;

        category.Name = request.Name;
        category.Description = request.Description;

        await db.SaveChangesAsync(ct);
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteCategoryAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null) return (false, false);
        if (category.BookCategories.Count > 0) return (true, true);

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Category deleted: {Id}", id);
        return (true, false);
    }
}
