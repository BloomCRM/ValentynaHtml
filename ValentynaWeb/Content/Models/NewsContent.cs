namespace ValentynaWeb.Content.Models;

public class NewsContent
{
    public List<NewsItem> Items { get; set; } = new();
}

public class NewsItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Date { get; set; } = "";
    public string? ImageCssClass { get; set; }
}
