using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public record CreateAuthorRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [MaxLength(2000)] string? Biography,
    DateOnly? BirthDate,
    [MaxLength(100)] string? Country);

public record UpdateAuthorRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [MaxLength(2000)] string? Biography,
    DateOnly? BirthDate,
    [MaxLength(100)] string? Country);

public record AuthorResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt);

public record AuthorDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    IReadOnlyList<AuthorBookResponse> Books);

public record AuthorBookResponse(
    int Id,
    string Title,
    string ISBN);
