namespace ValentynaWeb.Content.Models;

public class ReviewsContent
{
    public double? Rating { get; set; }
    public int? TotalCount { get; set; }
    public string? ProfileUrl { get; set; }
    public List<Review> Reviews { get; set; } = new();
}

public class Review
{
    public string AuthorName { get; set; } = "";
    public string? AuthorUrl { get; set; }
    public string AvatarUrl { get; set; } = "";
    public int Rating { get; set; } = 5;
    public string Text { get; set; } = "";
    /// <summary>"ua" / "en" — показувати лише у цій мовній версії; null — в обох.</summary>
    public string? Language { get; set; }
    public string? RelativeDate { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? GoogleUrl { get; set; }
}
