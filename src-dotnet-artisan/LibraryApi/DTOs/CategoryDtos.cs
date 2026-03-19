using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description);

public record UpdateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description);

public record CategoryResponse(
    int Id,
    string Name,
    string? Description);

public record CategoryDetailResponse(
    int Id,
    string Name,
    string? Description,
    int BookCount);
