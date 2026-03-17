using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<BookSummaryDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await bookService.GetAllAsync(search, category, available, sortBy, sortDir, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDetailDto>> GetById(int id, CancellationToken ct = default)
    {
        var book = await bookService.GetByIdAsync(id, ct);
        return book is not null ? Ok(book) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<BookDetailDto>> Create([FromBody] CreateBookRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Title is required." });
        if (string.IsNullOrWhiteSpace(request.ISBN))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "ISBN is required." });
        if (request.TotalCopies < 1)
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "TotalCopies must be at least 1." });
        if (request.AuthorIds is null || request.AuthorIds.Count == 0)
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "At least one AuthorId is required." });
        if (request.CategoryIds is null || request.CategoryIds.Count == 0)
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "At least one CategoryId is required." });

        var book = await bookService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookDetailDto>> Update(int id, [FromBody] UpdateBookRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Title is required." });
        if (string.IsNullOrWhiteSpace(request.ISBN))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "ISBN is required." });
        if (request.TotalCopies < 1)
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "TotalCopies must be at least 1." });

        var book = await bookService.UpdateAsync(id, request, ct);
        return book is not null ? Ok(book) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var (found, hasActiveLoans) = await bookService.DeleteAsync(id, ct);
        if (!found) return NotFound();
        if (hasActiveLoans) return Conflict(new ProblemDetails { Title = "Conflict", Detail = "Cannot delete book with active loans." });
        return NoContent();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<PaginatedResponse<LoanDto>>> GetLoans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await bookService.GetLoansAsync(id, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetReservations(int id, CancellationToken ct = default)
    {
        var result = await bookService.GetReservationsAsync(id, ct);
        return Ok(result);
    }
}
