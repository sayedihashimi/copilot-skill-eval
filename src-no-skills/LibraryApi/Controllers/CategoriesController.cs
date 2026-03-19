using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    /// <summary>List all categories with pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), 200)]
    public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetCategoriesAsync(page, pageSize));

    /// <summary>Get category details with book count.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategory(int id)
    {
        var cat = await _service.GetCategoryByIdAsync(id);
        return cat == null ? NotFound() : Ok(cat);
    }

    /// <summary>Create a new category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var cat = await _service.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = cat.Id }, cat);
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var cat = await _service.UpdateCategoryAsync(id, dto);
        return cat == null ? NotFound() : Ok(cat);
    }

    /// <summary>Delete a category (fails if category has books).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var (success, error) = await _service.DeleteCategoryAsync(id);
        if (!success && error!.Contains("not found")) return NotFound();
        if (!success) return BadRequest(new ProblemDetails { Title = "Cannot delete category", Detail = error, Status = 400 });
        return NoContent();
    }
}
