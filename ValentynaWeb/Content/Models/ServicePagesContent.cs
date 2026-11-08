namespace ValentynaWeb.Content.Models;

/// <summary>Сторінки послуг (/farbuvannia …) — Content/service-pages.json.</summary>
public class ServicePagesContent
{
    public List<ServicePage> Pages { get; set; } = new();

    /// <summary>Сторінка за адресою, без урахування регістру; null — такої немає (→ 404).</summary>
    public ServicePage? FindBySlug(string? slug) =>
        string.IsNullOrEmpty(slug)
            ? null
            : Pages.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));

    /// <summary>Опублікована сторінка за адресою; чернетки не повертаємо — з головної посилаємось лише на опубліковані.</summary>
    public ServicePage? FindPublished(string? slug) => FindBySlug(slug) is { Published: true } page ? page : null;

    /// <summary>Опубліковані сторінки категорії прайсу (id з services.json) — посилання внизу картки прайсу на головній.
    /// Їх може бути кілька: «Миття, укладки та догляд» → укладки і догляд.</summary>
    public IReadOnlyList<ServicePage> PublishedForCategory(string? categoryId) =>
        string.IsNullOrEmpty(categoryId)
            ? []
            : Pages.Where(p => p.Published && string.Equals(p.ServiceCategoryId, categoryId, StringComparison.Ordinal)).ToList();
}

public class ServicePage
{
    /// <summary>Адреса без слешів, латиницею: "farbuvannia" → /farbuvannia</summary>
    public string Slug { get; set; } = "";
    /// <summary>false — чернетка: відкривається за прямим посиланням, але з noindex, без sitemap і без посилань з головної</summary>
    public bool Published { get; set; }
    /// <summary>Id категорії в services.json, звідки беремо ціни: "coloring". null — цін на сторінці немає (завивка, брови)</summary>
    public string? ServiceCategoryId { get; set; }
    /// <summary>Які рядки категорії показати: ключ заголовка групи ("svc-group-wash") — уся група,
    /// ключ рядка ("styling-1") — цей рядок. null або порожньо — уся категорія</summary>
    public List<string>? PriceKeys { get; set; }
    /// <summary>Головна дія: "telegram" — «Надіслати фото в Telegram» (фарбування, завивка);
    /// "booking" — «Записатися» в онлайн-запис WLaunch, Telegram — другою кнопкою</summary>
    public string PrimaryAction { get; set; } = "telegram";
    /// <summary>&lt;title&gt; повністю (лише UA — Google індексує UA-версію)</summary>
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public LocalizedText H1 { get; set; } = new();
    public LocalizedText Lead { get; set; } = new();
    /// <summary>Фото в арці — шлях без розширення, як у &lt;responsive-img&gt;: "img/coloring-1"</summary>
    public string HeroImage { get; set; } = "";
    public string HeroAlt { get; set; } = "";
    /// <summary>Фото робіт сіткою, шляхи без розширення</summary>
    public List<string> Works { get; set; } = new();
    public LocalizedText PriceNote { get; set; } = new();
    /// <summary>Готовий текст у чаті Telegram (t.me/…?text=)</summary>
    public LocalizedText TelegramText { get; set; } = new();
    /// <summary>Підпис посилання в картці прайсу на головній</summary>
    public LocalizedText CardLinkText { get; set; } = new();
    public List<ServicePageStep> Steps { get; set; } = new();
    /// <summary>Свої цитати про цю послугу. Поки не виводяться: на сторінці — відгуки з Google, як на головній
    /// (_Reviews); коли відкриють Google Business Profile API, відгуки про послугу зробимо окремо</summary>
    public List<ServicePageReview> Reviews { get; set; } = new();
    public List<ServicePageFaq> Faq { get; set; } = new();
}

public class ServicePageStep
{
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Text { get; set; } = new();
}

/// <summary>Цитата з Google-відгуку — дослівно, автор як у Google</summary>
public class ServicePageReview
{
    public LocalizedText Text { get; set; } = new();
    public string Author { get; set; } = "";
}

/// <summary>Питання без відповіді (answer: null) на сторінці не показуємо</summary>
public class ServicePageFaq
{
    public LocalizedText Question { get; set; } = new();
    public LocalizedText? Answer { get; set; }
}
