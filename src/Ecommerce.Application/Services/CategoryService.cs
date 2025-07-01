using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(CategoryDto?, string?)> CreateCategoryAsync(CreateCategoryDto categoryDto)
    {
        var category = new Category { Name = categoryDto.Name };
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.CompleteAsync();
        var resultDto = new CategoryDto { Id = category.Id, Name = category.Name };
        return (resultDto, null);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return false;

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name });
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category == null ? null : new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<(CategoryDto?, string?)> UpdateCategoryAsync(Guid id, UpdateCategoryDto categoryDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return (null, "Category not found.");

        category.Name = categoryDto.Name;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.CompleteAsync();

        var resultDto = new CategoryDto { Id = category.Id, Name = category.Name };
        return (resultDto, null);
    }
}