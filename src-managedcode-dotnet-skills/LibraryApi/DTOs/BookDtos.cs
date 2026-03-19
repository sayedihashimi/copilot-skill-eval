using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public record BookSummaryDto(
    int Id,
    string Title,
    string ISBN,
    int TotalCopies,
    int AvailableCopies);

public record BookDto(
    int Id,
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string Language,
    int TotalCopies,
    int AvailableCopies,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<AuthorDto> Authors,
    IReadOnlyList<CategoryDto> Categories);

public record CreateBookDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; init; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; init; }

    [MaxLength(50)]
    public string Language { get; init; } = "English";

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; init; } = 1;

    public List<int> AuthorIds { get; init; } = [];
    public List<int> CategoryIds { get; init; } = [];
}

public record UpdateBookDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; init; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; init; }

    [MaxLength(50)]
    public string Language { get; init; } = "English";

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; init; } = 1;

    public List<int> AuthorIds { get; init; } = [];
    public List<int> CategoryIds { get; init; } = [];
}
