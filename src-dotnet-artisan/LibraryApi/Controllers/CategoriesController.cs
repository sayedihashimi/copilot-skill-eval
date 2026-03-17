using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct = default)
    {
        var result = await categoryService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDetailDto>> GetById(int id, CancellationToken ct = default)
    {
        var category = await categoryService.GetByIdAsync(id, ct);
        return category is not null ? Ok(category) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Name is required." });

        var category = await categoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Name is required." });

        var category = await categoryService.UpdateAsync(id, request, ct);
        return category is not null ? Ok(category) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var (found, hasBooks) = await categoryService.DeleteAsync(id, ct);
        if (!found) return NotFound();
        if (hasBooks) return Conflict(new ProblemDetails { Title = "Conflict", Detail = "Cannot delete category with associated books." });
        return NoContent();
    }
}
