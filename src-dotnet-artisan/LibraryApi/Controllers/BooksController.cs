using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetAllAsync(search, category, available, sortBy, sortDir, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDetailResponse>> GetById(int id)
    {
        var book = await bookService.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request)
    {
        var book = await bookService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookResponse>> Update(int id, UpdateBookRequest request)
    {
        var book = await bookService.UpdateAsync(id, request);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await bookService.DeleteAsync(id);
        if (!success)
        {
            return error == "Book not found."
                ? NotFound(new ProblemDetails { Title = error, Status = 404 })
                : Conflict(new ProblemDetails { Title = error, Status = 409 });
        }
        return NoContent();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<List<LoanResponse>>> GetBookLoans(int id)
    {
        var loans = await bookService.GetBookLoansAsync(id);
        return Ok(loans);
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<List<ReservationResponse>>> GetBookReservations(int id)
    {
        var reservations = await bookService.GetBookReservationsAsync(id);
        return Ok(reservations);
    }
}
