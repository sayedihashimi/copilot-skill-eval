using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthorsController(IAuthorService authorService) : ControllerBase
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
        var author = await authorService.GetAuthorByIdAsync(id);
        return author is not null ? Ok(author) : NotFound();
    }

    /// <summary>Create a new author.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorRequest request)
    {
        var author = await authorService.CreateAuthorAsync(request);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    /// <summary>Update an existing author.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuthor(int id, [FromBody] UpdateAuthorRequest request)
    {
        var author = await authorService.UpdateAuthorAsync(id, request);
        return author is not null ? Ok(author) : NotFound();
    }

    /// <summary>Delete an author (fail if the author has any books).</summary>
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
