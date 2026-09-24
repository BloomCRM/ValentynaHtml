using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Content;

public interface IReviewsProvider
{
    Task<ReviewsContent> GetAsync();
}
