using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string? Description);

public record CategoryDetailDto(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<BookSummaryDto> Books);

public record CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}

public record UpdateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
