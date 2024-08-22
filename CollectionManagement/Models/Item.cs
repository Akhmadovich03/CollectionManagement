namespace CollectionManagement.Models;

public class Item
{
    public int Id { get; set; }

    public int CollectionId { get; set; }
    public virtual Collection Collection { get; set; }

    public string Name { get; set; }

    public string? CustomFieldsData { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}