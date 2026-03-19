using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _db;

    public CategoryService(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page, int pageSize)
    {
        var total = await _db.Categories.CountAsync();
        var items = await _db.Categories.OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CategoryDto
            {
                Id = c.Id, Name = c.Name, Description = c.Description,
                BookCount = c.BookCategories.Count
            }).ToListAsync();

        return new PagedResult<CategoryDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        return await _db.Categories.Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id, Name = c.Name, Description = c.Description,
                BookCount = c.BookCategories.Count
            }).FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var cat = new Category { Name = dto.Name, Description = dto.Description };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return (await GetCategoryByIdAsync(cat.Id))!;
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return null;
        cat.Name = dto.Name;
        cat.Description = dto.Description;
        await _db.SaveChangesAsync();
        return (await GetCategoryByIdAsync(id))!;
    }

    public async Task<(bool Success, string? Error)> DeleteCategoryAsync(int id)
    {
        var cat = await _db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null) return (false, "Category not found.");
        if (cat.BookCategories.Any()) return (false, "Cannot delete category that has books associated.");
        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
