using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize);
    Task<AuthorResponse?> GetAuthorByIdAsync(int id);
    Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request);
    Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request);
    Task<bool> DeleteAuthorAsync(int id);
}
