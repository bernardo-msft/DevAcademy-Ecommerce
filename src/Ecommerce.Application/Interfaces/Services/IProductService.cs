using Ecommerce.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(Guid? categoryId = null);
    Task<(ProductDto? Product, string? ErrorMessage)> CreateProductAsync(CreateProductDto productDto);
    Task<(ProductDto? Product, string? ErrorMessage)> UpdateProductAsync(Guid id, UpdateProductDto productDto);
    Task<bool> DeleteProductAsync(Guid id);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, Guid? categoryId = null);
}