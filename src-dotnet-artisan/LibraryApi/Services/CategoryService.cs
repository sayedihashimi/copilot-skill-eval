using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryDetailResponse?> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class CategoryService(LibraryDbContext db) : ICategoryService
{
    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        return await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<CategoryDetailResponse?> GetByIdAsync(int id)
    {
        return await db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailResponse(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return null;

        category.Name = request.Name;
        category.Description = request.Description;

        await db.SaveChangesAsync();

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var category = await db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return (false, "Category not found.");
        if (category.BookCategories.Count > 0) return (false, "Cannot delete category with associated books.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return (true, null);
    }
}
