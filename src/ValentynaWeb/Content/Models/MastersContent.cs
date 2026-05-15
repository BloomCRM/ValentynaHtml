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
}
