using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public ProductService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<(ProductDto? Product, string? ErrorMessage)> CreateProductAsync(CreateProductDto productDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
        if (category == null) return (null, "Category not found.");

        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
            CategoryId = productDto.CategoryId
        };

        if (productDto.ImageFile != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(productDto.ImageFile.FileName)}";
            product.ImageUrl = await _fileStorageService.UploadFileAsync(productDto.ImageFile, fileName);
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CompleteAsync();

        var createdProduct = await _unitOfWork.Products.GetByIdWithCategoryAsync(product.Id);
        return (MapProductToDto(createdProduct!), null);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(Guid? categoryId = null)
    {
        var products = await _unitOfWork.Products.GetAllWithCategoryAsync(categoryId);
        return products.Select(MapProductToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(id);
        return product == null ? null : MapProductToDto(product);
    }

    public async Task<(ProductDto? Product, string? ErrorMessage)> UpdateProductAsync(Guid id, UpdateProductDto productDto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return (null, "Product not found.");

        var category = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
        if (category == null) return (null, "Category not found.");

        if (productDto.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var existingFileName = Path.GetFileName(new Uri(product.ImageUrl).AbsolutePath);
                await _fileStorageService.DeleteFileAsync(existingFileName);
            }
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(productDto.ImageFile.FileName)}";
            product.ImageUrl = await _fileStorageService.UploadFileAsync(productDto.ImageFile, fileName);
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.StockQuantity = productDto.StockQuantity;
        product.CategoryId = productDto.CategoryId;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.CompleteAsync();

        var updatedProduct = await _unitOfWork.Products.GetByIdWithCategoryAsync(id);
        return (MapProductToDto(updatedProduct!), null);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, Guid? categoryId = null)
    {
        var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm, categoryId);
        return products.Select(MapProductToDto);
    }

    private static ProductDto MapProductToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "N/A"
        };
    }
}