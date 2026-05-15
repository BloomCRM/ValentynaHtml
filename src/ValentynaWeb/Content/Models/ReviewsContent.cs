namespace ValentynaWeb.Content.Models;

public class ReviewsContent
{
    public List<Review> Reviews { get; set; } = new();
}

public class Review
{
    public string AuthorName { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public int Rating { get; set; } = 5;
    public string Text { get; set; } = "";
    public string? GoogleUrl { get; set; }
}
