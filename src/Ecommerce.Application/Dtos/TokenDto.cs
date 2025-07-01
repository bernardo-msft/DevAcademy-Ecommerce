namespace Ecommerce.Application.Dtos;

public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}