using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

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
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<BookAuthorResponse>? Authors = null,
    List<BookCategoryResponse>? Categories = null);

public sealed record BookAuthorResponse(int Id, string FirstName, string LastName);
public sealed record BookCategoryResponse(int Id, string Name);

public sealed record CreateBookRequest
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
    public string? Language { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int TotalCopies { get; init; }

    public List<int> AuthorIds { get; init; } = [];
    public List<int> CategoryIds { get; init; } = [];
}

public sealed record UpdateBookRequest
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
    public string? Language { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int TotalCopies { get; init; }

    public List<int> AuthorIds { get; init; } = [];
    public List<int> CategoryIds { get; init; } = [];
}
