using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuthorResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await authorService.GetAllAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDetailResponse>> GetById(int id)
    {
        var author = await authorService.GetByIdAsync(id);
        return author is null ? NotFound() : Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> Create(CreateAuthorRequest request)
    {
        var author = await authorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AuthorResponse>> Update(int id, UpdateAuthorRequest request)
    {
        var author = await authorService.UpdateAsync(id, request);
        return author is null ? NotFound() : Ok(author);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await authorService.DeleteAsync(id);
        if (!success)
        {
            return error == "Author not found."
                ? NotFound(new ProblemDetails { Title = error, Status = 404 })
                : Conflict(new ProblemDetails { Title = error, Status = 409 });
        }
        return NoContent();
    }
}
