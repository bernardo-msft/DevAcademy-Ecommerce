using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(CreateCategoryDto createCategoryDto)
    {
        var (category, errorMessage) = await _categoryService.CreateCategoryAsync(createCategoryDto);
        if (errorMessage != null) return BadRequest(new { Message = errorMessage });
        return CreatedAtAction(nameof(GetById), new { id = category!.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryDto updateCategoryDto)
    {
        var (category, errorMessage) = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
        if (errorMessage != null) return BadRequest(new { Message = errorMessage });
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _categoryService.DeleteCategoryAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}