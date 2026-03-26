using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public class CategoryService(LibraryDbContext db, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(int page, int pageSize)
    {
        var totalCount = await db.Categories.CountAsync();
        var items = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync();

        return new PaginatedResponse<CategoryResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
    {
        return await db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if (await db.Categories.AnyAsync(c => c.Name == request.Name))
            throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

        var category = new Category { Name = request.Name, Description = request.Description };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        logger.LogInformation("Category created: {CategoryId} {Name}", category.Id, category.Name);

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return null;

        if (await db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id))
            throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;
        await db.SaveChangesAsync();

        logger.LogInformation("Category updated: {CategoryId}", id);

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Count != 0)
            throw new InvalidOperationException("Cannot delete a category that has associated books.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        logger.LogInformation("Category deleted: {CategoryId}", id);
        return true;
    }
}
