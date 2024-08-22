namespace CollectionManagement.Models;

public class Comment
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public virtual Item Item { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; }

    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }
}