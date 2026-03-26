using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController(ICategoryService categoryService, IValidator<CreateCategoryRequest> createValidator, IValidator<UpdateCategoryRequest> updateValidator) : ControllerBase
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
        var result = await categoryService.GetCategoryByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await categoryService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, result);
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await categoryService.UpdateCategoryAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a category (fails if category has any books).</summary>
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
