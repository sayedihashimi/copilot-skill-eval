using LibraryApi.Models;

namespace LibraryApi.DTOs;

// ── Pagination ──
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// ── Author DTOs ──
public sealed record AuthorDto(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt);

public sealed record AuthorDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    IReadOnlyList<BookSummaryDto> Books);

public sealed record CreateAuthorRequest(
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country);

public sealed record UpdateAuthorRequest(
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country);

// ── Category DTOs ──
public sealed record CategoryDto(int Id, string Name, string? Description);

public sealed record CategoryDetailDto(int Id, string Name, string? Description, int BookCount);

public sealed record CreateCategoryRequest(string Name, string? Description);

public sealed record UpdateCategoryRequest(string Name, string? Description);

// ── Book DTOs ──
public sealed record BookSummaryDto(
    int Id,
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string Language,
    int TotalCopies,
    int AvailableCopies);

public sealed record BookDetailDto(
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

public sealed record CreateBookRequest(
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string? Language,
    int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds);

public sealed record UpdateBookRequest(
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string? Language,
    int TotalCopies);

// ── Patron DTOs ──
public sealed record PatronDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    MembershipType MembershipType,
    bool IsActive,
    DateOnly MembershipDate);

public sealed record PatronDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType,
    bool IsActive,
    DateOnly MembershipDate,
    DateTime CreatedAt,
    int ActiveLoans,
    decimal UnpaidFines);

public sealed record CreatePatronRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType);

public sealed record UpdatePatronRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType);

// ── Loan DTOs ──
public sealed record LoanDto(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount);

public sealed record CreateLoanRequest(int BookId, int PatronId);

// ── Reservation DTOs ──
public sealed record ReservationDto(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    ReservationStatus Status,
    int QueuePosition);

public sealed record CreateReservationRequest(int BookId, int PatronId);

// ── Fine DTOs ──
public sealed record FineDto(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status);
