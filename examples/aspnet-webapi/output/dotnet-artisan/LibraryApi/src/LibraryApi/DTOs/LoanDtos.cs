using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

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

public sealed record CreateLoanRequest
{
    [Required]
    public int BookId { get; init; }

    [Required]
    public int PatronId { get; init; }
}
