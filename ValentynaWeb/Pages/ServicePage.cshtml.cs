using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Сторінка послуги (/farbuvannia …). Тексти — з service-pages.json, ціни — з категорії services.json,
/// майстри — з masters.json (у кого в services є ця категорія). Невідома адреса → 404.
/// Чернетка (published: false) відкривається за прямим посиланням, але з noindex.
/// </summary>
public class ServicePageModel : PageModel
{
    private const string TelegramFallback = "https://t.me/valentyna_perukarnia";

    private readonly IContentProvider _content;
    private readonly IReviewsProvider _reviews;

    public ServicePageModel(IContentProvider content, IReviewsProvider reviews)
    {
        _content = content;
        _reviews = reviews;
    }

    public ServicePage Service { get; private set; } = new();
    /// <summary>Адреса без gclid/utm і малими літерами — для rel=canonical</summary>
    public string CanonicalUrl { get; private set; } = "";
    public PriceScale Prices { get; private set; } = PriceScale.From([]);
    /// <summary>JSON-LD сторінки: Service з цінами + хлібні крихти; вставляється в &lt;head&gt;</summary>
    public string JsonLd { get; private set; } = "";

    public SalonContent Salon { get; private set; } = new();
    public string PhoneDisplay { get; private set; } = "";
    public IReadOnlyList<IndexModel.ScheduleLine> Schedule { get; private set; } = [];
    /// <summary>Telegram з готовим текстом українською</summary>
    public string TelegramUrl { get; private set; } = TelegramFallback;
    /// <summary>Те саме з англійським текстом — для data-i18n-href-en; null — EN-тексту немає</summary>
    public string? TelegramUrlEn { get; private set; }
    public IReadOnlyList<Master> Masters { get; private set; } = [];
    /// <summary>Лише питання з відповіддю</summary>
    public IReadOnlyList<ServicePageFaq> Faq { get; private set; } = [];
    /// <summary>Рейтинг у Google «4,9»; null — Google API не відповів</summary>
    public string? RatingText { get; private set; }
    /// <summary>Усі відгуки в Google; null — немає ні профілю, ні карти</summary>
    public string? ReviewsUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var service = _content.ServicePages.FindBySlug(slug);
        if (service is null) return NotFound();

        Service = service;
        var category = _content.Services.Categories.FirstOrDefault(c => c.Id == service.ServiceCategoryId);
        Prices = PriceScale.From(category?.Services ?? []);

        Salon = _content.Salon;
        PhoneDisplay = IndexModel.FormatPhone(Salon.Phone);
        Schedule = Salon.OpeningHours.Select(IndexModel.ToScheduleLine).ToList();

        var telegram = Salon.Socials.Telegram ?? TelegramFallback;
        TelegramUrl = WithText(telegram, service.TelegramText.Ua);
        TelegramUrlEn = service.TelegramText.EnAttr is { } en ? WithText(telegram, en) : null;

        Masters = _content.Masters.Masters.Where(m => m.Services.Contains(service.ServiceCategoryId)).ToList();
        Faq = service.Faq.Where(f => !string.IsNullOrWhiteSpace(f.Answer?.Ua)).ToList();

        var reviews = await _reviews.GetAsync();
        RatingText = reviews.Rating?.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',');
        ReviewsUrl = reviews.ProfileUrl ?? Salon.Socials.GoogleMaps;

        var home = $"https://{Request.Host}/";
        CanonicalUrl = home + service.Slug.ToLowerInvariant();
        JsonLd = BuildJsonLd(home);
        return Page();
    }

    // t.me/<акаунт>?text=… відкриває чат з уже набраним повідомленням — адмін бачить, що людина з сайту
    private static string WithText(string url, string text) =>
        string.IsNullOrWhiteSpace(text) ? url : $"{url}?text={Uri.EscapeDataString(text)}";

    // Service + BreadcrumbList; виконавець — HairSalon з _JsonLd.cshtml за його @id
    private string BuildJsonLd(string home)
    {
        var service = new JsonObject
        {
            ["@type"] = "Service",
            ["name"] = Service.H1.Ua,
            ["serviceType"] = Service.H1.Ua,
            ["url"] = CanonicalUrl,
            ["provider"] = new JsonObject { ["@id"] = home + "#salon" },
            ["areaServed"] = new JsonObject { ["@type"] = "City", ["name"] = "Київ" }
        };

        var priced = Prices.Rows.Where(r => r.From is not null).ToList();
        if (priced.Count > 0)
        {
            service["offers"] = new JsonObject
            {
                ["@type"] = "AggregateOffer",
                ["priceCurrency"] = "UAH",
                ["lowPrice"] = priced.Min(r => r.From!.Value),
                ["highPrice"] = Prices.Max
            };
        }

        var breadcrumbs = new JsonObject
        {
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = new JsonArray(
                new JsonObject { ["@type"] = "ListItem", ["position"] = 1, ["name"] = "Головна", ["item"] = home },
                new JsonObject { ["@type"] = "ListItem", ["position"] = 2, ["name"] = Service.H1.Ua, ["item"] = CanonicalUrl })
        };

        return new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = new JsonArray(service, breadcrumbs)
        }.ToJsonString();
    }
}
