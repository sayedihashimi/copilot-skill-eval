using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public record LoanDto(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount,
    DateTime CreatedAt);

public record CreateLoanDto
{
    [Required]
    public int BookId { get; init; }

    [Required]
    public int PatronId { get; init; }
}

public record ReservationDto(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime ReservationDate,
    DateTime ExpirationDate,
    ReservationStatus Status,
    int QueuePosition,
    DateTime CreatedAt);

public record CreateReservationDto
{
    [Required]
    public int BookId { get; init; }

    [Required]
    public int PatronId { get; init; }
}

public record FineDto(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status,
    DateTime CreatedAt);
