namespace ValentynaWeb.Content.Models;

public class MastersContent
{
    public List<Master> Masters { get; set; } = new();
}

public class Master
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public string? Instagram { get; set; }
    /// <summary>Id категорій прайсу, які робить майстер (напр. "coloring") — для блоку «Майстри» на сторінці послуги</summary>
    public List<string> Services { get; set; } = new();
    /// <summary>Кілька слів про досвід; ранг пишемо в Role, фото — в AvatarUrl</summary>
    public LocalizedText? About { get; set; }
}
