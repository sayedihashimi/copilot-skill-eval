using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext context, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<PagedResult<CategoryResponse>> GetCategoriesAsync(int page, int pageSize)
    {
        var totalCount = await context.Categories.CountAsync();
        var items = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();

        return new PagedResult<CategoryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CategoryDetailResponse?> GetCategoryByIdAsync(int id)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                BookCount = c.BookCategories.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if (await context.Categories.AnyAsync(c => c.Name == request.Name))
            throw new InvalidOperationException($"A category with name '{request.Name}' already exists.");

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();
        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

        return new CategoryResponse { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null) return null;

        if (await context.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id))
            throw new InvalidOperationException($"A category with name '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;
        await context.SaveChangesAsync();
        logger.LogInformation("Updated category {CategoryId}", id);

        return new CategoryResponse { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await context.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Count != 0)
            throw new InvalidOperationException("Cannot delete a category that has books. Remove the books from this category first.");

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted category {CategoryId}", id);
        return true;
    }
}
