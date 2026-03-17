using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AuthorDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await authorService.GetAllAsync(search, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDetailDto>> GetById(int id, CancellationToken ct = default)
    {
        var author = await authorService.GetByIdAsync(id, ct);
        return author is not null ? Ok(author) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create([FromBody] CreateAuthorRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "FirstName and LastName are required." });

        var author = await authorService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AuthorDto>> Update(int id, [FromBody] UpdateAuthorRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "FirstName and LastName are required." });

        var author = await authorService.UpdateAsync(id, request, ct);
        return author is not null ? Ok(author) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var (found, hasBooks) = await authorService.DeleteAsync(id, ct);
        if (!found) return NotFound();
        if (hasBooks) return Conflict(new ProblemDetails { Title = "Conflict", Detail = "Cannot delete author with associated books." });
        return NoContent();
    }
}
