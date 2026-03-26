using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public sealed record AuthorResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    List<AuthorBookResponse>? Books = null);

public sealed record AuthorBookResponse(int Id, string Title, string ISBN);

public sealed record CreateAuthorRequest
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

public sealed record UpdateAuthorRequest
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
