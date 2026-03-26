using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>List all categories with pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await categoryService.GetCategoriesAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get category details with count of books.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>Create a new category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await categoryService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await categoryService.UpdateCategoryAsync(id, request);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>Delete a category (fails if category has books).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}
