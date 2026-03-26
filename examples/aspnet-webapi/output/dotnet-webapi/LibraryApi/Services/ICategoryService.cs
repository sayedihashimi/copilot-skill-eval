using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<CategoryDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct);
    Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
