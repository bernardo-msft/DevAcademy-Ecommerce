using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<(CategoryDto? Category, string? ErrorMessage)> CreateCategoryAsync(CreateCategoryDto categoryDto);
    Task<(CategoryDto? Category, string? ErrorMessage)> UpdateCategoryAsync(Guid id, UpdateCategoryDto categoryDto);
    Task<bool> DeleteCategoryAsync(Guid id);
}