using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IContentProvider _content;
    private readonly IReviewsProvider _reviews;
    private ServicePagesContent _servicePages = new();

    public IndexModel(IConfiguration config, IContentProvider content, IReviewsProvider reviews)
    {
        _config = config;
        _content = content;
        _reviews = reviews;
    }

    public string BookingUrl { get; private set; } = "#";
    public IReadOnlyList<ServiceCategory> ServiceCategories { get; private set; } = [];
    public SalonContent Salon { get; private set; } = new();
    public string PhoneDisplay { get; private set; } = "";
    public IReadOnlyList<ScheduleLine> Schedule { get; private set; } = [];
    public ReviewsContent Reviews { get; private set; } = new();
    public IReadOnlyList<NewsItem> UpcomingNews { get; private set; } = [];
    public IReadOnlyList<NewsItem> PastNews { get; private set; } = [];
    /// <summary>Найближча подія — для крапки біля «Події» в меню і плашки в банері; null — нічого не показуємо</summary>
    public NewsItem? NextEvent { get; private set; }
    public string NextEventWhen { get; private set; } = "";
    public string NextEventWhenEn { get; private set; } = "";
    public IReadOnlyList<ServicePhotoCategory> ServicePhotos { get; private set; } = [];
    /// <summary>Фото карток «Послуг» для перегляду-сторіз</summary>
    public ServiceStories Stories { get; private set; } = new([]);

    /// <summary>Опубліковані сторінки послуг категорії прайсу (id з services.json) — посилання внизу картки; чернетки не показуємо</summary>
    public IReadOnlyList<ServicePage> PublishedPagesFor(string? categoryId) => _servicePages.PublishedForCategory(categoryId);

    public async Task OnGetAsync()
    {
        ServicePhotos = _content.ServicePhotos.Categories;
        _servicePages = _content.ServicePages;

        var today = KyivToday();
        var news = _content.News.Items;
        UpcomingNews = news.Where(n => n.EventDate >= today).OrderBy(n => n.EventDate).ToList();
        PastNews = news.Where(n => n.EventDate < today).OrderByDescending(n => n.EventDate).ToList();
        NextEvent = UpcomingNews.FirstOrDefault();
        if (NextEvent is not null)
            (NextEventWhen, NextEventWhenEn) = EventWhen(NextEvent.EventDate, today);

        BookingUrl = BookingLink.From(_config);
        // прайс — до сторіз: кнопка в сторіз залежить від того, чи є на сторінці послуги ціни
        ServiceCategories = _content.Services.Categories;
        Stories = new ServiceStories(ServicePhotos.Select(c => new ServiceStories.Category(
            c.Title, c.TitleKey, null, c.Cover, c.Photos, StoryButton(c.Page))).ToList());

        Salon = _content.Salon;
        PhoneDisplay = FormatPhone(Salon.Phone);
        Schedule = Salon.OpeningHours.Select(ToScheduleLine).ToList();
        Reviews = await _reviews.GetAsync();
    }

    // Кнопка внизу сторіз: картка з опублікованою сторінкою веде туди — «Ціни та запис», якщо на сторінці є ціни,
    // інакше «Детальніше» (завивка, брови); решта — в онлайн-запис WLaunch
    private ServiceStories.Cta StoryButton(string? pageSlug) =>
        _servicePages.FindPublished(pageSlug) is not { } page
            ? new(BookingUrl, null, "hero-book", "Записатися", External: true)
            : HasPrices(page)
                ? new("/" + page.Slug, null, "stories-page", "Ціни та запис", External: false)
                : new("/" + page.Slug, null, "stories-more", "Детальніше", External: false);

    // ті самі ціни, що покаже сторінка: її категорія і priceKeys
    private bool HasPrices(ServicePage page) =>
        ServiceCategories.FirstOrDefault(c => c.Id == page.ServiceCategoryId) is { } category
        && PriceScale.From(category.Services, page.PriceKeys).Rows.Count > 0;

    // Ключ перекладу під українську множину: 1 відгук / 2 відгуки / 5 відгуків
    public static (string Key, string Label) ReviewsCountLabel(int n) =>
        (n % 10, n % 100) switch
        {
            (1, not 11) => ("reviews-count-one", "відгук у Google Maps"),
            (>= 2 and <= 4, not (>= 12 and <= 14)) => ("reviews-count-few", "відгуки у Google Maps"),
            _ => ("reviews-count-many", "відгуків у Google Maps")
        };

    // "img/gallery 3" → "/img/webp/gallery-3" (до цього JS дописує -768.webp / -1280.webp), як у ResponsiveImageTagHelper
    public static string WebpBase(string path) =>
        "/img/webp/" + Path.GetFileNameWithoutExtension(path).Replace(" ", "-");

    // Місяці вручну, а не через CultureInfo("uk-UA"): у Docker-образі культура може бути недоступна
    private static readonly string[] MonthsUa =
        ["січня", "лютого", "березня", "квітня", "травня", "червня", "липня", "серпня", "вересня", "жовтня", "листопада", "грудня"];
    private static readonly string[] MonthsEn =
        ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    // «Сьогодні» / «Завтра» / «8 листопада» — коротко, для плашки в банері
    public static (string Ua, string En) EventWhen(DateOnly date, DateOnly today) =>
        (date.DayNumber - today.DayNumber) switch
        {
            0 => ("Сьогодні", "Today"),
            1 => ("Завтра", "Tomorrow"),
            _ => ($"{date.Day} {MonthsUa[date.Month - 1]}", $"{MonthsEn[date.Month - 1]} {date.Day}")
        };

    // Сервер у Docker живе в UTC, а "сьогодні" для подій — за київським часом
    private static DateOnly KyivToday()
    {
        var now = TimeZoneInfo.TryFindSystemTimeZoneById("Europe/Kyiv", out var kyiv)
            ? TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, kyiv)
            : DateTimeOffset.UtcNow;
        return DateOnly.FromDateTime(now.DateTime);
    }

    public record ScheduleLine(string TranslateKey, string Days, string Time);

    private static readonly Dictionary<string, string> DayAbbrUa = new()
    {
        ["Mon"] = "Пн", ["Tue"] = "Вт", ["Wed"] = "Ср", ["Thu"] = "Чт",
        ["Fri"] = "Пт", ["Sat"] = "Сб", ["Sun"] = "Нд"
    };

    // "+380637250589" → "+38 (063) 725-05-89"; інші формати — як є
    public static string FormatPhone(string phone) =>
        phone.Length == 13 && phone.StartsWith("+380")
            ? $"+38 ({phone[3..6]}) {phone[6..9]}-{phone[9..11]}-{phone[11..13]}"
            : phone;

    // ["Mon".."Sun"] → "Щодня"; ["Mon".."Fri"] → "Пн-Пт". EN-переклад — у site.js за ключем
    public static ScheduleLine ToScheduleLine(OpeningHours h)
    {
        var time = $"{h.Open} - {h.Close}";
        if (h.Days.Count == 7)
            return new ScheduleLine("schedule-daily", "Щодня", time);

        var first = h.Days.FirstOrDefault() ?? "";
        var last = h.Days.LastOrDefault() ?? "";
        var days = first == last
            ? DayAbbrUa.GetValueOrDefault(first, first)
            : $"{DayAbbrUa.GetValueOrDefault(first, first)}-{DayAbbrUa.GetValueOrDefault(last, last)}";
        return new ScheduleLine($"schedule-{first}-{last}".ToLowerInvariant(), days, time);
    }
}
