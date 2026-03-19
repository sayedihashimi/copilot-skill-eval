using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public record AuthorDto(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt);

public record AuthorDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    IReadOnlyList<BookSummaryDto> Books);

public record CreateAuthorDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; init; }

    public DateOnly? BirthDate { get; init; }

    [MaxLength(100)]
    public string? Country { get; init; }
}

public record UpdateAuthorDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; init; }

    public DateOnly? BirthDate { get; init; }

    [MaxLength(100)]
    public string? Country { get; init; }
}
