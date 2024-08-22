namespace CollectionManagement.Models;

public class Like
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public virtual Item Item { get; set; }

    public int LikedUserId { get; set; }
    public virtual User LikedUser { get; set; }

    public DateTime CreatedAt { get; set; }
}