using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController(IBookService bookService, IValidator<CreateBookRequest> createValidator, IValidator<UpdateBookRequest> updateValidator) : ControllerBase
{
    /// <summary>List books with search, filter by availability, pagination, and sorting.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BookResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await bookService.GetBooksAsync(search, category, available, sortBy, sortOrder, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get book details including authors, categories, and availability info.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBook(int id)
    {
        var result = await bookService.GetBookByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new book with author IDs and category IDs.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await bookService.CreateBookAsync(request);
        return CreatedAtAction(nameof(GetBook), new { id = result.Id }, result);
    }

    /// <summary>Update an existing book.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await bookService.UpdateBookAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a book (fails if the book has any active loans).</summary>
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
