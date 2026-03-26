using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    /// <summary>List authors with search by name and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuthorResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthors([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await authorService.GetAuthorsAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get author details including their books.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuthor(int id)
    {
        var author = await authorService.GetAuthorByIdAsync(id);
        return author is null ? NotFound() : Ok(author);
    }

    /// <summary>Create a new author.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorRequest request)
    {
        var author = await authorService.CreateAuthorAsync(request);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    /// <summary>Update an existing author.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuthor(int id, [FromBody] UpdateAuthorRequest request)
    {
        var author = await authorService.UpdateAuthorAsync(id, request);
        return author is null ? NotFound() : Ok(author);
    }

    /// <summary>Delete an author (fails if the author has any books).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        await authorService.DeleteAuthorAsync(id);
        return NoContent();
    }
}
