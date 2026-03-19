using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public record CreateLoanRequest(
    [Required] int BookId,
    [Required] int PatronId);

public record LoanResponse(
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

public record LoanDetailResponse(
    int Id,
    int BookId,
    string BookTitle,
    string BookISBN,
    int PatronId,
    string PatronName,
    string PatronEmail,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount,
    DateTime CreatedAt);
