using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// На які сторінки послуг посилатися з сайту (меню, картки «Послуг», підвал, прайс, сторіз, «Інші послуги»).
/// На живому сайті — лише опубліковані; у режимі розробки (локально) — і чернетки, щоб бачити сайт у зборі.
/// Порядок — як картки «Послуг» на головній (service-photos.json, поле page).
/// </summary>
public class ServicePageLinks
{
    private readonly IContentProvider _content;
    private readonly bool _includeDrafts;

    public ServicePageLinks(IContentProvider content, IHostEnvironment env)
    {
        _content = content;
        _includeDrafts = env.IsDevelopment();
    }

    /// <param name="Cover">Обкладинка картки «Послуг» цієї сторінки (без розширення, як у &lt;responsive-img&gt;); немає картки — перше фото сторінки</param>
    /// <param name="HasPrices">На сторінці є блок цін — підпис посилання «Ціни та запис», інакше «Детальніше»</param>
    public record Link(ServicePage Page, string Cover, bool HasPrices);

    /// <summary>Сторінки, на які можна посилатися, у порядку карток «Послуг»; сторінки без картки — у кінці, в порядку service-pages.json</summary>
    public IReadOnlyList<Link> All()
    {
        var cards = _content.ServicePhotos.Categories;
        int CardIndex(ServicePage page)
        {
            var i = cards.FindIndex(c => string.Equals(c.Page, page.Slug, StringComparison.OrdinalIgnoreCase));
            return i < 0 ? int.MaxValue : i;
        }

        return _content.ServicePages.Pages
            .Where(p => _includeDrafts || p.Published)
            .OrderBy(CardIndex)
            .Select(p => new Link(
                p,
                cards.FirstOrDefault(c => string.Equals(c.Page, p.Slug, StringComparison.OrdinalIgnoreCase))?.Cover ?? p.HeroImage,
                HasPrices(p)))
            .ToList();
    }

    // ті самі ціни, що покаже сторінка: її категорія прайсу і priceKeys
    private bool HasPrices(ServicePage page) =>
        _content.Services.Categories.FirstOrDefault(c => c.Id == page.ServiceCategoryId) is { } category
        && PriceScale.From(category.Services, page.PriceKeys).Rows.Count > 0;
}
