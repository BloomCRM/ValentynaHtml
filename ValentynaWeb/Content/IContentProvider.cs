using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Content;

public interface IContentProvider
{
    SalonContent Salon { get; }
    ServicesContent Services { get; }
    MastersContent Masters { get; }
    ReviewsContent Reviews { get; }
    GalleryContent Gallery { get; }
    NewsContent News { get; }
}
