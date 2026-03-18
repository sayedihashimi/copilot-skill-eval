using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<AuthorResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List authors")]
    [EndpointDescription("Returns a paginated list of authors, optionally filtered by name search.")]
    public async Task<ActionResult<PagedResponse<AuthorResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await authorService.GetAllAsync(search, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<AuthorDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get author by ID")]
    [EndpointDescription("Returns author details including their books.")]
    public async Task<ActionResult<AuthorDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await authorService.GetByIdAsync(id, cancellationToken);
        return author is null ? NotFound() : Ok(author);
    }

    [HttpPost]
    [ProducesResponseType<AuthorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Create an author")]
    [EndpointDescription("Creates a new author and returns the created resource.")]
    public async Task<ActionResult<AuthorResponse>> Create(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        var author = await authorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<AuthorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Update an author")]
    [EndpointDescription("Updates an existing author's information.")]
    public async Task<ActionResult<AuthorResponse>> Update(int id, UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        var author = await authorService.UpdateAsync(id, request, cancellationToken);
        return author is null ? NotFound() : Ok(author);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Delete an author")]
    [EndpointDescription("Deletes an author. Fails if the author has associated books.")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await authorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
