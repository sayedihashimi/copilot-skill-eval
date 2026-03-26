using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController(IAuthorService authorService, IValidator<CreateAuthorRequest> createValidator, IValidator<UpdateAuthorRequest> updateValidator) : ControllerBase
{
    /// <summary>List authors with search by name and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<AuthorResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthors([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await authorService.GetAuthorsAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get author details including their books.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuthor(int id)
    {
        var result = await authorService.GetAuthorByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new author.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await authorService.CreateAuthorAsync(request);
        return CreatedAtAction(nameof(GetAuthor), new { id = result.Id }, result);
    }

    /// <summary>Update an existing author.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAuthor(int id, [FromBody] UpdateAuthorRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await authorService.UpdateAuthorAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete an author (fails if the author has any books).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        await authorService.DeleteAuthorAsync(id);
        return NoContent();
    }
}
