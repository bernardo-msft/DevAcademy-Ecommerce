namespace Ecommerce.Application.Dtos;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateCategoryDto
{
    public required string Name { get; set; }
}

public class UpdateCategoryDto
{
    public required string Name { get; set; }
}