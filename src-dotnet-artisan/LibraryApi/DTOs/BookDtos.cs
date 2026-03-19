using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public record CreateBookRequest(
    [Required, MaxLength(300)] string Title,
    [Required, MaxLength(20)] string ISBN,
    [MaxLength(200)] string? Publisher,
    int? PublicationYear,
    [MaxLength(2000)] string? Description,
    int? PageCount,
    [MaxLength(50)] string? Language,
    [Range(1, int.MaxValue)] int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds);

public record UpdateBookRequest(
    [Required, MaxLength(300)] string Title,
    [Required, MaxLength(20)] string ISBN,
    [MaxLength(200)] string? Publisher,
    int? PublicationYear,
    [MaxLength(2000)] string? Description,
    int? PageCount,
    [MaxLength(50)] string? Language,
    [Range(1, int.MaxValue)] int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds);

public record BookResponse(
    int Id,
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string Language,
    int TotalCopies,
    int AvailableCopies);

public record BookDetailResponse(
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
    IReadOnlyList<BookAuthorResponse> Authors,
    IReadOnlyList<BookCategoryResponse> Categories);

public record BookAuthorResponse(int Id, string FirstName, string LastName);
public record BookCategoryResponse(int Id, string Name);
