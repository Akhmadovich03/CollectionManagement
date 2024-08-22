namespace CollectionManagement.Models.ViewModels;

public class ItemDTO
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public Collection Collection { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int LikesCount { get; set; }

    public bool IsUserLiked { get; set; }
}