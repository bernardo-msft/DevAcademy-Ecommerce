using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}