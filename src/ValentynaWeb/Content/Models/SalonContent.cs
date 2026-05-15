namespace ValentynaWeb.Content.Models;

public class SalonContent
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string TimeZone { get; set; } = "Europe/Kyiv";
    public string Currency { get; set; } = "UAH";
    public string Slogan { get; set; } = "";
    public List<OpeningHours> OpeningHours { get; set; } = new();
    public SocialLinks Socials { get; set; } = new();
    public FounderInfo Founder { get; set; } = new();
}

public class OpeningHours
{
    public List<string> Days { get; set; } = new();
    public string Open { get; set; } = "";
    public string Close { get; set; } = "";
}

public class SocialLinks
{
    public string? Instagram { get; set; }
    public string? Telegram { get; set; }
    public string? GoogleMaps { get; set; }
}

public class FounderInfo
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}
