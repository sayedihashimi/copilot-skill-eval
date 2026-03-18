using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<BookResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List books")]
    [EndpointDescription("Returns a paginated list of books with search, category filter, availability filter, and sorting.")]
    public async Task<ActionResult<PagedResponse<BookResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await bookService.GetAllAsync(search, category, available, sortBy, sortDirection, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<BookDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get book by ID")]
    [EndpointDescription("Returns book details including authors, categories, and availability.")]
    public async Task<ActionResult<BookDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    [ProducesResponseType<BookResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a book")]
    [EndpointDescription("Creates a new book with author and category associations.")]
    public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var book = await bookService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a book")]
    [EndpointDescription("Updates an existing book's information and associations.")]
    public async Task<ActionResult<BookResponse>> Update(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var book = await bookService.UpdateAsync(id, request, cancellationToken);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Delete a book")]
    [EndpointDescription("Deletes a book. Fails if the book has active loans.")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await bookService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/loans")]
    [ProducesResponseType<PagedResponse<LoanResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get book loan history")]
    [EndpointDescription("Returns the loan history for a specific book.")]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetBookLoans(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await bookService.GetBookLoansAsync(id, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}/reservations")]
    [ProducesResponseType<List<ReservationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get active book reservations")]
    [EndpointDescription("Returns the active reservations queue for a specific book.")]
    public async Task<ActionResult<List<ReservationResponse>>> GetBookReservations(int id, CancellationToken cancellationToken)
    {
        return Ok(await bookService.GetBookReservationsAsync(id, cancellationToken));
    }
}
