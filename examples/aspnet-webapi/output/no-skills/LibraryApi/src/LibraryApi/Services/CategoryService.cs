using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _db;

    public CategoryService(LibraryDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories.OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(int id)
    {
        var cat = await _db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null) return null;

        return new CategoryDetailDto
        {
            Id = cat.Id, Name = cat.Name, Description = cat.Description,
            BookCount = cat.BookCategories.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new BusinessRuleException($"A category with the name '{dto.Name}' already exists.");

        var category = new Category { Name = dto.Name, Description = dto.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return null;

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new BusinessRuleException($"A category with the name '{dto.Name}' already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        await _db.SaveChangesAsync();
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) throw new NotFoundException($"Category with ID {id} not found.");
        if (category.BookCategories.Any()) throw new BusinessRuleException("Cannot delete a category that has books. Remove the book associations first.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }
}
