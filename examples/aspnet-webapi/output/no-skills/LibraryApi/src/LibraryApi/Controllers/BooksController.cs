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

    /// <summary>List books with search, filter by availability, pagination, sorting</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? available, [FromQuery] string? sortBy, [FromQuery] string? sortDir, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, available, sortBy, sortDir, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get book details including authors, categories, and availability</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new book with author and category IDs</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] BookCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing book</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a book (fails if the book has active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get the loan history for a specific book</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetBookLoans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetBookLoansAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get the active reservations queue for a specific book</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(List<ReservationDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetBookReservations(int id)
    {
        var result = await _service.GetBookReservationsAsync(id);
        return Ok(result);
    }
}
