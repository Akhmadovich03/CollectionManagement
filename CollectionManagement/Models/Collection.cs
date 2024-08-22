using CollectionManagement.Models.Enums;

namespace CollectionManagement.Models;

public class Collection
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public virtual User? User { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public Category Category { get; set; }

    public string? ImageURL { get; set; }

    public string? CustomFields { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public virtual IEnumerable<Item> Items { get; set; }
}