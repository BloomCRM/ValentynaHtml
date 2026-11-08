using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Сторінка послуги (/farbuvannia …). Тексти — з service-pages.json, ціни — з категорії services.json,
/// майстри — з masters.json (у кого в services є ця категорія). Невідома адреса → 404.
/// Головна дія (primaryAction) — Telegram або онлайн-запис WLaunch.
/// Чернетка (published: false) відкривається за прямим посиланням, але з noindex.
/// </summary>
public class ServicePageModel : PageModel
{
    private const string TelegramFallback = "https://t.me/valentyna_perukarnia";

    private readonly IContentProvider _content;
    private readonly IReviewsProvider _reviews;
    private readonly IConfiguration _config;

    public ServicePageModel(IContentProvider content, IReviewsProvider reviews, IConfiguration config)
    {
        _content = content;
        _reviews = reviews;
        _config = config;
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
    /// <summary>Онлайн-запис WLaunch — головна кнопка сторінок з primaryAction "booking"</summary>
    public string BookingUrl { get; private set; } = "";
    /// <summary>Головна дія — «Записатися» (WLaunch), Telegram другою кнопкою; інакше — «Надіслати фото в Telegram»</summary>
    public bool IsBooking => Service.PrimaryAction == "booking";
    /// <summary>Роботи у форматі сторіз (клік по фото в «Роботах»)</summary>
    public ServiceStories Stories { get; private set; } = new([]);
    public IReadOnlyList<Master> Masters { get; private set; } = [];
    /// <summary>Лише питання з відповіддю</summary>
    public IReadOnlyList<ServicePageFaq> Faq { get; private set; } = [];
    /// <summary>Рейтинг і відгуки з Google — ті самі, що на головній (_RatingBadge, _Reviews)</summary>
    public ReviewsContent Reviews { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var service = _content.ServicePages.FindBySlug(slug);
        if (service is null) return NotFound();

        Service = service;
        var category = _content.Services.Categories.FirstOrDefault(c => c.Id == service.ServiceCategoryId);
        Prices = PriceScale.From(category?.Services ?? [], service.PriceKeys);

        Salon = _content.Salon;
        PhoneDisplay = IndexModel.FormatPhone(Salon.Phone);
        Schedule = Salon.OpeningHours.Select(IndexModel.ToScheduleLine).ToList();

        var telegram = Salon.Socials.Telegram ?? TelegramFallback;
        TelegramUrl = WithText(telegram, service.TelegramText.Ua);
        TelegramUrlEn = service.TelegramText.EnAttr is { } en ? WithText(telegram, en) : null;

        BookingUrl = BookingLink.From(_config);

        // кнопка внизу сторіз — та сама головна дія сторінки, що й скрізь тут
        var storiesButton = IsBooking
            ? new ServiceStories.Cta(BookingUrl, null, "hero-book", "Записатися", External: true)
            : new ServiceStories.Cta(TelegramUrl, TelegramUrlEn, "stories-tg", "Надіслати фото в Telegram", External: true);
        Stories = new ServiceStories([new ServiceStories.Category(
            service.H1.Ua, null, service.H1.EnAttr, service.HeroImage, service.Works, storiesButton)]);

        Masters = service.ServiceCategoryId is { } categoryId
            ? _content.Masters.Masters.Where(m => m.Services.Contains(categoryId)).ToList()
            : [];
        Faq = service.Faq.Where(f => !string.IsNullOrWhiteSpace(f.Answer?.Ua)).ToList();

        Reviews = await _reviews.GetAsync();

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
