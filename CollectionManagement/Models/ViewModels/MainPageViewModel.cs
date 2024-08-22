namespace CollectionManagement.Models.ViewModels;

public class MainPageViewModel
{
    public List<Collection> Collections { get; set; }

    public List<ItemDTO> Items { get; set; }

    public User? SignedInUser { get; set; }
}