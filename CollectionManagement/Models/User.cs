using CollectionManagement.Models.Enums;

namespace CollectionManagement.Models;

public class User
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsBlocked { get; set; }

    public Theme PageTheme { get; set; }

    public Language PageLanguage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}