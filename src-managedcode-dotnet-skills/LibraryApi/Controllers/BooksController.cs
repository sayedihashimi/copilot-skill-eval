using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await bookService.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        var book = await bookService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookDto>> Update(int id, UpdateBookDto dto)
    {
        var book = await bookService.UpdateAsync(id, dto);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await bookService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetBookLoansAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetReservations(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetBookReservationsAsync(id, page, pageSize);
        return Ok(result);
    }
}
