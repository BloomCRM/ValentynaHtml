namespace ValentynaWeb.Content.Models;

public class GalleryContent
{
    public List<GalleryItem> Items { get; set; } = new();
}

public class GalleryItem
{
    public string ImageUrl { get; set; } = "";
    public string Alt { get; set; } = "";
}
