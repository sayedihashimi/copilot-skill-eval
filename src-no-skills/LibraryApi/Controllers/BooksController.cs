using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service) => _service = service;

    /// <summary>List books with search, filter by availability and category, pagination, and sorting.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), 200)]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? search, [FromQuery] string? category,
        [FromQuery] bool? available, [FromQuery] string? sortBy,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetBooksAsync(search, category, available, sortBy, page, pageSize));

    /// <summary>Get book details including authors, categories, and availability.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _service.GetBookByIdAsync(id);
        return book == null ? NotFound() : Ok(book);
    }

    /// <summary>Create a new book with author and category IDs.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto)
    {
        var book = await _service.CreateBookAsync(dto);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    /// <summary>Update an existing book.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto dto)
    {
        var book = await _service.UpdateBookAsync(id, dto);
        return book == null ? NotFound() : Ok(book);
    }

    /// <summary>Delete a book (fails if book has active loans).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var (success, error) = await _service.DeleteBookAsync(id);
        if (!success && error!.Contains("not found")) return NotFound();
        if (!success) return BadRequest(new ProblemDetails { Title = "Cannot delete book", Detail = error, Status = 400 });
        return NoContent();
    }

    /// <summary>Get loan history for a specific book.</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    public async Task<IActionResult> GetBookLoans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetBookLoansAsync(id, page, pageSize));

    /// <summary>Get active reservations queue for a specific book.</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), 200)]
    public async Task<IActionResult> GetBookReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetBookReservationsAsync(id, page, pageSize));
}
