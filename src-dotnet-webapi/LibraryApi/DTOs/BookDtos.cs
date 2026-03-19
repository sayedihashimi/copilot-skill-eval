using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

// --- Book DTOs ---

public sealed record BookResponse(
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
    IReadOnlyList<string> Authors,
    IReadOnlyList<string> Categories,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record BookSummaryResponse(
    int Id,
    string Title,
    string ISBN,
    int TotalCopies,
    int AvailableCopies
);

public sealed record BookDetailResponse(
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
    IReadOnlyList<AuthorResponse> Authors,
    IReadOnlyList<CategoryResponse> Categories,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateBookRequest
{
    [Required, MaxLength(300)]
    public required string Title { get; init; }

    [Required]
    public required string ISBN { get; init; }

    [MaxLength(200)]
    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; init; }

    [MaxLength(100)]
    public string? Language { get; init; }

    [Required, Range(1, int.MaxValue)]
    public required int TotalCopies { get; init; }

    public IReadOnlyList<int>? AuthorIds { get; init; }
    public IReadOnlyList<int>? CategoryIds { get; init; }
}

public sealed record UpdateBookRequest
{
    [Required, MaxLength(300)]
    public required string Title { get; init; }

    [Required]
    public required string ISBN { get; init; }

    [MaxLength(200)]
    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; init; }

    [MaxLength(100)]
    public string? Language { get; init; }

    [Required, Range(1, int.MaxValue)]
    public required int TotalCopies { get; init; }

    public IReadOnlyList<int>? AuthorIds { get; init; }
    public IReadOnlyList<int>? CategoryIds { get; init; }
}
