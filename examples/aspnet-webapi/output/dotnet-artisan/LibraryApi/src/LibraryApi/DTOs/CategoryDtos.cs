using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public sealed record CategoryResponse(int Id, string Name, string? Description, int? BookCount = null);

public sealed record CreateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}

public sealed record UpdateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
