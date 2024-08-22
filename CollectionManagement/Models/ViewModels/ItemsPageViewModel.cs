namespace CollectionManagement.Models.ViewModels;

public class ItemsPageViewModel
{
    public Collection Collection { get; set; }

    public List<ItemDTO> Items { get; set; }

    public User? User { get; set; }
}