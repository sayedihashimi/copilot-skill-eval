using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(int page, int pageSize);
    Task<CategoryResponse?> GetCategoryByIdAsync(int id);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);
}
