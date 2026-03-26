using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

// --- Loan DTOs ---

public sealed record CreateLoanRequest
{
    [Required]
    public required int BookId { get; init; }

    [Required]
    public required int PatronId { get; init; }
}

public sealed record LoanResponse(
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
