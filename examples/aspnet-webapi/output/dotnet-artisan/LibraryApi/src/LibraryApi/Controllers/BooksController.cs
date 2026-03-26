using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class BooksController(IBookService bookService) : ControllerBase
{
    /// <summary>List books with search, filter by availability, pagination, and sorting.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BookResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? search,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetBooksAsync(search, available, sortBy, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get book details including authors, categories, and availability info.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await bookService.GetBookByIdAsync(id);
        return book is not null ? Ok(book) : NotFound();
    }

    /// <summary>Create a new book.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        var book = await bookService.CreateBookAsync(request);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    /// <summary>Update an existing book.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        var book = await bookService.UpdateBookAsync(id, request);
        return book is not null ? Ok(book) : NotFound();
    }

    /// <summary>Delete a book (fail if the book has any active loans).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        await bookService.DeleteBookAsync(id);
        return NoContent();
    }

    /// <summary>Get the loan history for a specific book.</summary>
    [HttpGet("{id:int}/loans")]
    [ProducesResponseType(typeof(PaginatedResponse<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookLoans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetBookLoansAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get the active reservations queue for a specific book.</summary>
    [HttpGet("{id:int}/reservations")]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetBookReservationsAsync(id, page, pageSize);
        return Ok(result);
    }
}
