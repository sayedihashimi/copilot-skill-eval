using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>List all categories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await categoryService.GetCategoriesAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get category details with count of books in the category.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        return category is not null ? Ok(category) : NotFound();
    }

    /// <summary>Create a new category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await categoryService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await categoryService.UpdateCategoryAsync(id, request);
        return category is not null ? Ok(category) : NotFound();
    }

    /// <summary>Delete a category (fail if category has any books).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}
