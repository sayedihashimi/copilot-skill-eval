using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext db) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct)
    {
        return await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(ct);
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailDto(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null) return null;

        category.Name = request.Name;
        category.Description = request.Description;

        await db.SaveChangesAsync(ct);
        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null) return (false, false);
        if (category.BookCategories.Count > 0) return (true, true);

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        return (true, false);
    }
}
