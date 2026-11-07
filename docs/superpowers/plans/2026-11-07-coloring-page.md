# Сторінка «Фарбування» — план реалізації

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Сторінка `/farbuvannia` під рекламу Google Ads (роботи → шкала цін → запис через Telegram), яка спершу живе чернеткою за прямим посиланням, а після `published: true` отримує посилання з головної.

**Architecture:** Одна сторінка-шаблон `Pages/ServicePage.cshtml` з маршрутом `/{slug}`. Контент — у новому `Content/service-pages.json` через наявний `IContentProvider` (hot reload). Ціни беруться з категорії `services.json`. Головна знаходить опубліковану сторінку за id категорії. Переклад EN — наявний механізм `data-i18n-en` у `site.js`.

**Tech Stack:** .NET 8, ASP.NET Core Razor Pages, System.Text.Json / JsonNode, vanilla JS (`site.js`, `service-stories.js`), CSS без збирача.

**Спека:** `docs/superpowers/specs/2026-11-07-coloring-page-design.md`. Макети: `docs/coloring-mockups/g-combined.html` (сторінка), `docs/coloring-mockups/h-homepage.html` (головна).

## Global Constraints

- .NET 8, Razor Pages, namespace `ValentynaWeb…`. Нових NuGet-пакетів не додаємо.
- Усе I/O — async, без `.Result` / `.Wait()`.
- Гілка `feat/coloring-page`. Коміти з префіксами `feat:` / `fix:` / `docs:`; кожне повідомлення закінчується рядком `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- `published` у `service-pages.json` у всіх комітах лишається `false`. Публікація — окремий крок після показу партнерам, не в цьому плані.
- Поки сторінка — чернетка, головна виглядає й працює як зараз.
- Host у адресах і JSON-LD — з `Request.Host` / `Context.Request.Host`, не з конфігу.
- Картинки — `<responsive-img src="img/x" …>` (без розширення). `alt` тег-хелпер не екранує, тому в alt не можна лапок `"`.
- Класи нової сторінки з префіксом `sp-`, стилі — в окремому `wwwroot/css/service-page.css`. У `site.css` додається лише `.price-more`.
- UA-текст стоїть у розмітці, EN — в атрибуті `data-i18n-en` (для посилань — `data-i18n-href-en`). Порожнього `data-i18n-en=""` не буває: для цього є `LocalizedText.EnAttr`.
- Push і PR — тільки після згоди користувача.

## Як перевіряємо

Тестового проєкту в репо немає, і спека його не додає. Тому кожна задача перевіряється на запущеному застосунку: `curl` для статусів і ASCII-маркерів, браузер (панель або DevTools) для тексту й вигляду.

- Запуск (окремий термінал або фоном), з кореня репо:
  ```bash
  dotnet run --project ValentynaWeb --urls http://localhost:5080
  ```
  Чекати рядок `Now listening on: http://localhost:5080`. Далі у прикладах `BASE=http://localhost:5080`.
- **Після кожної зміни C#, `.cshtml` або JSON-прапорця застосунок перезапускаємо.** Причина — output cache: сторінки кешуються на 5 хвилин, `sitemap.xml` — на годину.
- Razor кодує кирилицю в HTML як `&#x…;`, тому кириличні тексти перевіряємо в браузері (`document.title` тощо), а не `grep`-ом.
- Збірка: `dotnet build ValentynaWeb/ValentynaWeb.csproj`. Очікування — `0 Error(s)`.

## Карта файлів

| Файл | Що робить | Задача |
|---|---|---|
| `ValentynaWeb/Content/Models/LocalizedText.cs` (новий) | Пара UA/EN + `EnAttr` для атрибута | 1 |
| `ValentynaWeb/Content/Models/ServicePagesContent.cs` (новий) | Моделі `service-pages.json`, пошук за slug і за категорією | 1 |
| `ValentynaWeb/Content/service-pages.json` (новий) | Контент сторінки «Фарбування», `published: false` | 1 |
| `ValentynaWeb/Content/IContentProvider.cs`, `JsonContentProvider.cs` | + `ServicePages` з hot reload | 1 |
| `ValentynaWeb/Pages/ServicePage.cshtml(.cs)` (нові) | Маршрут `/{slug}`, дані сторінки, розмітка | 1, 2, 4 |
| `ValentynaWeb/Pages/Shared/_Layout.cshtml` | `PageTitle`, `Description`, секція `Head`, `OwnContactBar` | 1 |
| `ValentynaWeb/Pages/PriceScale.cs` (новий) | Розбір цін категорії в шкалу | 2 |
| `ValentynaWeb/Pages/Shared/_JsonLd.cshtml` | `@id` у HairSalon | 2 |
| `ValentynaWeb/Pages/Sitemap.cshtml.cs` | + опубліковані сторінки послуг | 2 |
| `ValentynaWeb/wwwroot/js/site.js` | Перевірки на відсутні елементи, `data-i18n-href-en`, ключ `stories-page` | 3 |
| `ValentynaWeb/Content/Models/MastersContent.cs` | + `Services`, `About` | 4 |
| `ValentynaWeb/Pages/Index.cshtml.cs` | `FormatPhone`, `ToScheduleLine` публічні; `PublishedPageFor` | 4, 5 |
| `ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml` (новий) | Коротка шапка | 4 |
| `ValentynaWeb/wwwroot/css/service-page.css` (новий) | Стилі сторінки | 4 |
| `ValentynaWeb/Content/Models/ServicePhotosContent.cs`, `service-photos.json` | + `serviceId` | 5 |
| `ValentynaWeb/Pages/Index.cshtml`, `Shared/_ServiceStories.cshtml`, `wwwroot/js/service-stories.js`, `wwwroot/css/site.css` | Посилання з картки прайсу і сторіс | 5 |

---

### Task 1: Сторінка-чернетка за адресою `/farbuvannia`

Після задачі: `/farbuvannia` → 200 з правильними title, description, canonical і `noindex`. Невідома адреса → 404. Головна не змінилась.

**Files:**
- Create: `ValentynaWeb/Content/Models/LocalizedText.cs`
- Create: `ValentynaWeb/Content/Models/ServicePagesContent.cs`
- Create: `ValentynaWeb/Content/service-pages.json`
- Modify: `ValentynaWeb/Content/IContentProvider.cs`
- Modify: `ValentynaWeb/Content/JsonContentProvider.cs`
- Create: `ValentynaWeb/Pages/ServicePage.cshtml.cs`
- Create: `ValentynaWeb/Pages/ServicePage.cshtml`
- Modify: `ValentynaWeb/Pages/Shared/_Layout.cshtml` (рядки 9–10, 29, 32, 109–111)

**Interfaces:**
- Produces:
  - `LocalizedText { string Ua; string? En; string? EnAttr }`
  - `ServicePagesContent.FindBySlug(string) → ServicePage?`
  - `ServicePagesContent.FindPublishedByCategory(string?) → ServicePage?`
  - `IContentProvider.ServicePages`
  - `ServicePageModel { ServicePage Service; string CanonicalUrl }`
  - ViewData-ключі layout: `PageTitle`, `Description`, `OwnContactBar`; секція `Head`

- [ ] **Step 1: Знімок головної і перевірка, що сторінки ще немає**

Запустити застосунок на поточному коді (див. «Як перевіряємо»), потім:

```bash
BASE=http://localhost:5080
curl -s $BASE/ | grep -o -E '<title>[^<]*</title>|<meta name="description"[^>]*>' > /tmp/main-head-before.txt
wc -l < /tmp/main-head-before.txt
curl -s -o /dev/null -w '%{http_code}\n' $BASE/farbuvannia
```

Expected: `2`, потім `404`. Зупинити застосунок.

- [ ] **Step 2: Модель двомовного тексту**

Create `ValentynaWeb/Content/Models/LocalizedText.cs`:

```csharp
using System.Text.Json.Serialization;

namespace ValentynaWeb.Content.Models;

/// <summary>
/// Текст двома мовами. UA стоїть у розмітці (її індексує Google),
/// EN — в атрибуті data-i18n-en, який підставляє перемикач мови в site.js.
/// </summary>
public class LocalizedText
{
    public string Ua { get; set; } = "";
    public string? En { get; set; }

    /// <summary>Значення для data-i18n-en: null, коли EN немає, — тоді Razor атрибут не виводить і лишається UA.</summary>
    [JsonIgnore]
    public string? EnAttr => string.IsNullOrWhiteSpace(En) ? null : En;
}
```

- [ ] **Step 3: Моделі сторінок послуг**

Create `ValentynaWeb/Content/Models/ServicePagesContent.cs`:

```csharp
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

    /// <summary>Опублікована сторінка категорії прайсу (id з services.json); чернетки не повертаємо.</summary>
    public ServicePage? FindPublishedByCategory(string? categoryId) =>
        string.IsNullOrEmpty(categoryId)
            ? null
            : Pages.FirstOrDefault(p => p.Published && string.Equals(p.ServiceCategoryId, categoryId, StringComparison.Ordinal));
}

public class ServicePage
{
    /// <summary>Адреса без слешів, латиницею: "farbuvannia" → /farbuvannia</summary>
    public string Slug { get; set; } = "";
    /// <summary>false — чернетка: відкривається за прямим посиланням, але з noindex, без sitemap і без посилань з головної</summary>
    public bool Published { get; set; }
    /// <summary>Id категорії в services.json, звідки беремо ціни: "coloring"</summary>
    public string ServiceCategoryId { get; set; } = "";
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
```

- [ ] **Step 4: Контент сторінки**

Create `ValentynaWeb/Content/service-pages.json`:

```json
{
  "pages": [
    {
      "slug": "farbuvannia",
      "published": false,
      "serviceCategoryId": "coloring",
      "title": "Фарбування волосся у центрі Києва — Перукарня Валентина",
      "description": "Фарбування волосся в центрі Києва: коріння, тонування, складні техніки, вихід з чорного. Ціни від 1500 грн. Надішліть фото — майстер підкаже техніку.",
      "h1": { "ua": "Фарбування волосся", "en": "Hair coloring" },
      "lead": {
        "ua": "Від тонування коренів до виходу з чорного. Надішліть фото волосся, і майстер підкаже техніку та орієнтовну ціну.",
        "en": "From root toning to coming out of black. Send a photo of your hair and a stylist will suggest a technique and an estimated price."
      },
      "heroImage": "img/coloring-1",
      "heroAlt": "Клієнтка з бірюзовим фарбуванням на довгому волоссі",
      "works": [
        "img/coloring-3",
        "img/coloring-12",
        "img/coloring-5",
        "img/coloring-2",
        "img/coloring-13",
        "img/coloring-7",
        "img/coloring-16",
        "img/coloring-14"
      ],
      "priceNote": {
        "ua": "Ціна залежить від довжини й густоти волосся, його стану і рангу майстра. Точну суму майстер назве після фото або на консультації.",
        "en": "The price depends on hair length and thickness, its condition and the stylist's level. The stylist will give the exact price after seeing a photo or at a consultation."
      },
      "telegramText": {
        "ua": "Привіт! Пишу з сайту щодо фарбування.",
        "en": "Hi! I'm writing from your website about hair coloring."
      },
      "cardLinkText": { "ua": "Детальніше про фарбування", "en": "More about hair coloring" },
      "steps": [
        {
          "title": { "ua": "Надішліть фото волосся в Telegram", "en": "Send a photo of your hair on Telegram" },
          "text": {
            "ua": "Краще при денному світлі, спереду і ззаду. Напишіть, який колір хочете.",
            "en": "Daylight works best, front and back. Tell us what colour you'd like."
          }
        },
        {
          "title": { "ua": "Майстер підкаже техніку й ціну", "en": "A stylist suggests the technique and price" },
          "text": { "ua": "Відповідаємо щодня з 10:00 до 21:00.", "en": "We reply daily from 10:00 to 21:00." }
        },
        {
          "title": { "ua": "Консультація або одразу фарбування", "en": "Consultation or straight to coloring" },
          "text": {
            "ua": "Можна спершу прийти на консультацію в салон, а можна одразу записатися на фарбування.",
            "en": "Come to the salon for a consultation first, or book the coloring right away."
          }
        }
      ],
      "reviews": [],
      "faq": [
        {
          "question": { "ua": "Скільки коштує консультація?", "en": "How much is a consultation?" },
          "answer": null
        },
        {
          "question": { "ua": "Чи можна вийти з чорного в блонд?", "en": "Can I go from black to blonde?" },
          "answer": {
            "ua": "Так, це окрема послуга. Скільки сеансів знадобиться, майстер скаже після фото: це залежить від того, як довго і чим фарбували волосся.",
            "en": "Yes, it's a separate service. The stylist will tell you how many sessions it takes after seeing a photo: it depends on how long and with what your hair was dyed."
          }
        },
        {
          "question": { "ua": "Скільки триває фарбування?", "en": "How long does coloring take?" },
          "answer": null
        },
        {
          "question": { "ua": "Якими фарбами працюєте?", "en": "Which hair dyes do you use?" },
          "answer": null
        }
      ]
    }
  ]
}
```

Три питання з `"answer": null` — це те, на що має відповісти Валик. На сторінці вони не показуються, поки немає відповіді.

- [ ] **Step 5: Провайдер контенту**

Modify `ValentynaWeb/Content/IContentProvider.cs`. Після рядка `    ServicePhotosContent ServicePhotos { get; }` додати:

```csharp
    ServicePagesContent ServicePages { get; }
```

Modify `ValentynaWeb/Content/JsonContentProvider.cs`. Після `        Watch<ServicePhotosContent>("service-photos.json");` додати:

```csharp
        Watch<ServicePagesContent>("service-pages.json");
```

Після `    public ServicePhotosContent ServicePhotos => Get<ServicePhotosContent>("service-photos.json");` додати:

```csharp
    public ServicePagesContent ServicePages => Get<ServicePagesContent>("service-pages.json");
```

- [ ] **Step 6: Модель сторінки**

Create `ValentynaWeb/Pages/ServicePage.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Сторінка послуги (/farbuvannia …). Тексти — з service-pages.json, невідома адреса → 404.
/// Чернетка (published: false) відкривається за прямим посиланням, але з noindex і без посилань з головної.
/// </summary>
public class ServicePageModel : PageModel
{
    private readonly IContentProvider _content;

    public ServicePageModel(IContentProvider content)
    {
        _content = content;
    }

    public ServicePage Service { get; private set; } = new();
    /// <summary>Адреса без gclid/utm і малими літерами — для rel=canonical</summary>
    public string CanonicalUrl { get; private set; } = "";

    public IActionResult OnGet(string slug)
    {
        var service = _content.ServicePages.FindBySlug(slug);
        if (service is null) return NotFound();

        Service = service;
        CanonicalUrl = $"https://{Request.Host}/{service.Slug.ToLowerInvariant()}";
        return Page();
    }
}
```

- [ ] **Step 7: Мінімальна розмітка сторінки**

Create `ValentynaWeb/Pages/ServicePage.cshtml`:

```cshtml
@page "/{slug}"
@model ServicePageModel
@{
    ViewData["PageTitle"] = Model.Service.Title;
    ViewData["Description"] = Model.Service.Description;
    ViewData["OwnContactBar"] = true;
}
@section Head {
    <link rel="canonical" href="@Model.CanonicalUrl" />
    @if (!Model.Service.Published)
    {
        <meta name="robots" content="noindex" />
    }
}

<main class="sp-page">
  <h1 data-i18n-en="@Model.Service.H1.EnAttr">@Model.Service.H1.Ua</h1>
</main>
```

- [ ] **Step 8: Layout — title, description, секція Head, власна нижня панель**

Modify `ValentynaWeb/Pages/Shared/_Layout.cshtml`.

Replace (рядки 9–10):

```cshtml
    <meta name="description" content="Перукарня Валентина — стрижки, фарбування, укладка в центрі Києва" />
    <title>@(ViewData["Title"] != null ? ViewData["Title"] + " — " : "")Перукарня Валентина — стрижки, фарбування, догляд у центрі Києва</title>
```

with:

```cshtml
    @* Сторінки послуг задають свої title і description; головна — як було *@
    @if (ViewData["Description"] is string description)
    {
        <meta name="description" content="@description" />
    }
    else
    {
        <meta name="description" content="Перукарня Валентина — стрижки, фарбування, укладка в центрі Києва" />
    }
    @if (ViewData["PageTitle"] is string pageTitle)
    {
        <title>@pageTitle</title>
    }
    else
    {
        <title>@(ViewData["Title"] != null ? ViewData["Title"] + " — " : "")Перукарня Валентина — стрижки, фарбування, догляд у центрі Києва</title>
    }
```

Replace:

```cshtml
    @await RenderSectionAsync("Styles", required: false)
```

with:

```cshtml
    @await RenderSectionAsync("Styles", required: false)
    @* canonical, noindex для чернеток і JSON-LD сторінки послуги *@
    @await RenderSectionAsync("Head", required: false)
```

Replace `<body>` with:

```cshtml
<body class="@(ViewData["OwnContactBar"] is true ? "has-contact-bar" : null)">
```

Replace:

```cshtml
    <a href="@bookingUrl" target="_blank" rel="noopener" class="sticky-book-btn" aria-label="Записатися онлайн">
        Записатися
    </a>
```

with:

```cshtml
    @* На сторінках послуг своя нижня панель (Telegram + дзвінок): липка «Записатися» веде в WLaunch, де цих послуг немає *@
    @if (ViewData["OwnContactBar"] is not true)
    {
        <a href="@bookingUrl" target="_blank" rel="noopener" class="sticky-book-btn" aria-label="Записатися онлайн">
            Записатися
        </a>
    }
```

- [ ] **Step 9: Збірка**

Run: `dotnet build ValentynaWeb/ValentynaWeb.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 10: Перевірка адрес і head**

Перезапустити застосунок, потім:

```bash
BASE=http://localhost:5080
for p in farbuvannia FARBUVANNIA nema-takoi robots.txt sitemap.xml health ""; do printf '%-12s %s\n' "/$p" "$(curl -s -o /dev/null -w '%{http_code}' $BASE/$p)"; done
curl -s $BASE/FARBUVANNIA | grep -o '<link rel="canonical"[^>]*>'
curl -s $BASE/farbuvannia | grep -c 'name="robots" content="noindex"'
curl -s $BASE/farbuvannia | grep -c 'sticky-book-btn'
curl -s $BASE/ | grep -c 'sticky-book-btn'
curl -s $BASE/ | grep -o -E '<title>[^<]*</title>|<meta name="description"[^>]*>' | diff /tmp/main-head-before.txt - && echo MAIN-HEAD-SAME
```

Expected:
```
/farbuvannia 200
/FARBUVANNIA 200
/nema-takoi  404
/robots.txt  200
/sitemap.xml 200
/health      200
/            200
<link rel="canonical" href="https://localhost:5080/farbuvannia" />
1
0
1
MAIN-HEAD-SAME
```

У браузері на `http://localhost:5080/farbuvannia` виконати в консолі:

```js
[document.title, document.querySelector('meta[name=description]').content, document.body.className]
```

Expected: `["Фарбування волосся у центрі Києва — Перукарня Валентина", "Фарбування волосся в центрі Києва: коріння, тонування, складні техніки, вихід з чорного. Ціни від 1500 грн. Надішліть фото — майстер підкаже техніку.", "has-contact-bar"]`.

На цьому кроці в консолі буде помилка з `site.js` (`Cannot read properties of null (reading 'addEventListener')`). Її прибирає Task 3.

- [ ] **Step 11: Commit**

```bash
git add ValentynaWeb/Content/Models/LocalizedText.cs ValentynaWeb/Content/Models/ServicePagesContent.cs ValentynaWeb/Content/service-pages.json ValentynaWeb/Content/IContentProvider.cs ValentynaWeb/Content/JsonContentProvider.cs ValentynaWeb/Pages/ServicePage.cshtml ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Pages/Shared/_Layout.cshtml
git commit -m "feat: сторінка послуги /{slug} з service-pages.json — «Фарбування» чернеткою (noindex)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Шкала цін, JSON-LD і sitemap

Після задачі:
- у `<head>` сторінки є JSON-LD `Service` з `lowPrice` 1500 / `highPrice` 13000 і хлібні крихти;
- HairSalon на всіх сторінках має `@id`;
- sitemap містить лише опубліковані сторінки (зараз — тільки головну).

**Files:**
- Create: `ValentynaWeb/Pages/PriceScale.cs`
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml.cs` (повна заміна)
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml` (секція `Head`)
- Modify: `ValentynaWeb/Pages/Shared/_JsonLd.cshtml:29`
- Modify: `ValentynaWeb/Pages/Sitemap.cshtml.cs` (повна заміна)

**Interfaces:**
- Consumes:
  - `ServicePagesContent.Pages`, `ServicePage.Published`, `ServicePage.ServiceCategoryId` (Task 1)
  - `ServicesContent.Categories[].Services` (`ServiceItem.Name`, `TranslateKey`, `PriceDisplay`, `IsGroupHeader`)
- Produces:
  - `PriceScale.From(IEnumerable<ServiceItem>) → PriceScale`
  - властивості `Rows`, `Max`, `Ticks`
  - `Pct(int) → string` (з крапкою, без `%`)
  - `static Num(int) → string` («13 000»)
  - `PriceScale.Row(string Name, string? TranslateKey, string PriceText, int? From, int? To)`
  - `ServicePageModel.Prices`, `ServicePageModel.JsonLd`

- [ ] **Step 1: Перевірка, що JSON-LD сторінки ще немає**

Застосунок на коді після Task 1:

```bash
BASE=http://localhost:5080
curl -s $BASE/farbuvannia | grep -c '"@type":"Service"'
curl -s $BASE/ | grep -c '#salon'
```

Expected: `0` і `0`.

- [ ] **Step 2: Шкала цін**

Create `ValentynaWeb/Pages/PriceScale.cs`:

```csharp
using System.Globalization;
using System.Text.RegularExpressions;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Шкала цін категорії для сторінки послуги: рядки «від–до», відсортовані за «від»,
/// кінець шкали — найбільша ціна «до», позначки осі — кожні 5 000 грн.
/// Ціни беремо з services.json (той самий прайс, що й на головній).
/// </summary>
public partial class PriceScale
{
    private const int TickStep = 5000;
    private static readonly NumberFormatInfo Spaces = new() { NumberGroupSeparator = " " };

    /// <summary>From/To = null — ціну не вдалося розібрати: рядок показує текст із прайсу без смужки.</summary>
    public record Row(string Name, string? TranslateKey, string PriceText, int? From, int? To);

    public IReadOnlyList<Row> Rows { get; }
    /// <summary>Кінець шкали; 0 — жодної ціни не розібрано</summary>
    public int Max { get; }
    /// <summary>Позначки осі: 0, 5000, 10000 … (менші за Max)</summary>
    public IReadOnlyList<int> Ticks { get; }

    private PriceScale(List<Row> rows)
    {
        Rows = rows;
        Max = rows.Max(r => r.To) ?? 0;
        Ticks = Max == 0
            ? []
            : Enumerable.Range(0, Max / TickStep + 1).Select(i => i * TickStep).Where(v => v < Max).ToList();
    }

    public static PriceScale From(IEnumerable<ServiceItem> services) =>
        new(services
            .Where(s => !s.IsGroupHeader)
            .Select(ToRow)
            .OrderBy(r => r.From ?? int.MaxValue)
            .ToList());

    /// <summary>Відсоток від кінця шкали для style="left:…%" — з крапкою незалежно від культури сервера</summary>
    public string Pct(int value) =>
        (Max == 0 ? 0 : value * 100.0 / Max).ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>13000 → «13 000» (нерозривний пробіл)</summary>
    public static string Num(int value) => value.ToString("#,0", Spaces);

    // "2000-2500 грн" → 2000…2500; "1500 грн" → 1500…1500; інше — без чисел (текст як є)
    private static Row ToRow(ServiceItem s)
    {
        var numbers = Digits().Matches(s.PriceDisplay)
            .Select(m => int.Parse(m.Value, CultureInfo.InvariantCulture))
            .ToList();

        int? lo = null, hi = null;
        if (numbers.Count == 1)
        {
            lo = hi = numbers[0];
        }
        else if (numbers.Count == 2 && numbers[0] <= numbers[1])
        {
            (lo, hi) = (numbers[0], numbers[1]);
        }

        var text = lo is null || hi is null ? s.PriceDisplay
            : lo == hi ? $"{Num(lo.Value)} грн"
            : $"{Num(lo.Value)}–{Num(hi.Value)} грн";

        return new Row(s.Name, s.TranslateKey, text, lo, hi);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex Digits();
}
```

- [ ] **Step 3: Модель сторінки з цінами і JSON-LD**

Replace the whole `ValentynaWeb/Pages/ServicePage.cshtml.cs` with:

```csharp
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Сторінка послуги (/farbuvannia …). Тексти — з service-pages.json, ціни — з категорії services.json.
/// Невідома адреса → 404. Чернетка (published: false) відкривається за прямим посиланням, але з noindex.
/// </summary>
public class ServicePageModel : PageModel
{
    private readonly IContentProvider _content;

    public ServicePageModel(IContentProvider content)
    {
        _content = content;
    }

    public ServicePage Service { get; private set; } = new();
    /// <summary>Адреса без gclid/utm і малими літерами — для rel=canonical</summary>
    public string CanonicalUrl { get; private set; } = "";
    public PriceScale Prices { get; private set; } = PriceScale.From([]);
    /// <summary>JSON-LD сторінки: Service з цінами + хлібні крихти; вставляється в &lt;head&gt;</summary>
    public string JsonLd { get; private set; } = "";

    public IActionResult OnGet(string slug)
    {
        var service = _content.ServicePages.FindBySlug(slug);
        if (service is null) return NotFound();

        Service = service;
        var category = _content.Services.Categories.FirstOrDefault(c => c.Id == service.ServiceCategoryId);
        Prices = PriceScale.From(category?.Services ?? []);

        var home = $"https://{Request.Host}/";
        CanonicalUrl = home + service.Slug.ToLowerInvariant();
        JsonLd = BuildJsonLd(home);
        return Page();
    }

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
```

- [ ] **Step 4: JSON-LD у head сторінки**

Modify `ValentynaWeb/Pages/ServicePage.cshtml`. Replace:

```cshtml
    @if (!Model.Service.Published)
    {
        <meta name="robots" content="noindex" />
    }
}
```

with:

```cshtml
    @if (!Model.Service.Published)
    {
        <meta name="robots" content="noindex" />
    }
    <script type="application/ld+json">@Html.Raw(Model.JsonLd)</script>
}
```

- [ ] **Step 5: `@id` у HairSalon**

Modify `ValentynaWeb/Pages/Shared/_JsonLd.cshtml`. Після рядка `        ["@type"]     = "HairSalon",` додати:

```cshtml
        ["@id"]       = $"https://{Context.Request.Host}/#salon",
```

- [ ] **Step 6: Sitemap з опублікованими сторінками**

Replace the whole `ValentynaWeb/Pages/Sitemap.cshtml.cs` with:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using ValentynaWeb.Content;

namespace ValentynaWeb.Pages;

[OutputCache(Duration = 3600)]
public class SitemapModel : PageModel
{
    private readonly IContentProvider _content;

    public SitemapModel(IContentProvider content)
    {
        _content = content;
    }

    public IActionResult OnGet()
    {
        var host = Request.Host.Value;
        var lastmod = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // головна + опубліковані сторінки послуг; чернетки (published: false) у карту сайту не потрапляють
        var urls = new List<string> { UrlEntry(host, "", lastmod, "weekly", "1.0") };
        urls.AddRange(_content.ServicePages.Pages
            .Where(p => p.Published)
            .Select(p => UrlEntry(host, p.Slug.ToLowerInvariant(), lastmod, "monthly", "0.8")));

        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            {string.Join("\n", urls)}
            </urlset>
            """;
        return Content(xml, "application/xml");
    }

    private static string UrlEntry(string host, string path, string lastmod, string changefreq, string priority) => $"""
          <url>
            <loc>https://{host}/{path}</loc>
            <lastmod>{lastmod}</lastmod>
            <changefreq>{changefreq}</changefreq>
            <priority>{priority}</priority>
          </url>
        """;
}
```

- [ ] **Step 7: Збірка**

Run: `dotnet build ValentynaWeb/ValentynaWeb.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 8: Перевірка**

Перезапустити застосунок:

```bash
BASE=http://localhost:5080
curl -s $BASE/farbuvannia | grep -o '"lowPrice":[0-9]*,"highPrice":[0-9]*'
curl -s $BASE/farbuvannia | grep -c '"@type":"BreadcrumbList"'
curl -s $BASE/farbuvannia | grep -o '"provider":{"@id":"[^"]*"}'
curl -s $BASE/ | grep -o '"@id":"[^"]*#salon"'
curl -s $BASE/sitemap.xml | grep -o '<loc>[^<]*</loc>'
curl -s $BASE/sitemap.xml | head -1
```

Expected:
```
"lowPrice":1500,"highPrice":13000
1
"provider":{"@id":"https://localhost:5080/#salon"}
"@id":"https://localhost:5080/#salon"
<loc>https://localhost:5080/</loc>
<?xml version="1.0" encoding="UTF-8"?>
```

Скопіювати вміст `<script type="application/ld+json">` з `/farbuvannia` у https://validator.schema.org (Code snippet). Очікування: 0 помилок для `Service` і `BreadcrumbList`.

- [ ] **Step 9: Commit**

```bash
git add ValentynaWeb/Pages/PriceScale.cs ValentynaWeb/Pages/ServicePage.cshtml ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Pages/Shared/_JsonLd.cshtml ValentynaWeb/Pages/Sitemap.cshtml.cs
git commit -m "feat: шкала цін, JSON-LD Service і sitemap для сторінок послуг" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: `site.js` працює на сторінках без елементів головної

Після задачі:
- на `/farbuvannia` у консолі немає помилок, перемикач мови працює;
- посилання з `data-i18n-href-en` міняють адресу при перемиканні мови;
- на головній бургер і лайтбокс працюють як раніше.

**Files:**
- Modify: `ValentynaWeb/wwwroot/js/site.js` (рядки 1–16, `translations` біля `stories-next`, `switchLanguage`, ініціалізація лайтбокса). Файл у UTF-8 з BOM і CRLF — зберегти як є.

**Interfaces:**
- Produces:
  - атрибут `data-i18n-href-en="<EN-адреса>"` на `<a>`;
  - ключ перекладу `stories-page` («Ціни та запис» / «Prices & booking»), його використовує Task 5.

- [ ] **Step 1: Перевірка, що зараз скрипт падає**

Застосунок на коді після Task 2. Відкрити `http://localhost:5080/farbuvannia` у браузері.

Expected:
- у консолі помилка `TypeError: Cannot read properties of null (reading 'addEventListener')` з `site.js:5`;
- виклик `switchLanguage('en')` у консолі падає з `ReferenceError` (`translations` не ініціалізовано).

- [ ] **Step 2: Бургер-меню лише там, де воно є**

Replace у `site.js`:

```js
burgerBtn.addEventListener('click', () => {
  mobileMenu.classList.add('active');
});

closeBtn.addEventListener('click', () => {
  mobileMenu.classList.remove('active');
});

// Пункт меню веде до розділу на цій же сторінці — меню закриваємо, інакше скрол відбувається під ним
mobileMenu.querySelectorAll('.mobile-nav-link').forEach(link => {
  link.addEventListener('click', () => mobileMenu.classList.remove('active'));
});
```

with:

```js
// Бургер-меню є лише на головній; на сторінках послуг його немає — тоді нічого не вішаємо,
// інакше скрипт падає тут і не працюють ні переклад, ні події аналітики
if (burgerBtn && closeBtn && mobileMenu) {
  burgerBtn.addEventListener('click', () => {
    mobileMenu.classList.add('active');
  });

  closeBtn.addEventListener('click', () => {
    mobileMenu.classList.remove('active');
  });

  // Пункт меню веде до розділу на цій же сторінці — меню закриваємо, інакше скрол відбувається під ним
  mobileMenu.querySelectorAll('.mobile-nav-link').forEach(link => {
    link.addEventListener('click', () => mobileMenu.classList.remove('active'));
  });
}
```

- [ ] **Step 3: Ключ перекладу для кнопки в сторіс**

Replace `    'stories-next': 'Далі',` with:

```js
    'stories-next': 'Далі',
    'stories-page': 'Ціни та запис',
```

Replace `    'stories-next': 'Next',` with:

```js
    'stories-next': 'Next',
    'stories-page': 'Prices & booking',
```

- [ ] **Step 4: EN-адреса посилання при перемиканні мови**

У `switchLanguage` replace:

```js
  document.querySelectorAll('[data-i18n-en]').forEach(element => {
    if (element.dataset.i18nUa === undefined) element.dataset.i18nUa = element.textContent;
    element.textContent = lang === 'en' ? element.dataset.i18nEn : element.dataset.i18nUa;
  });
```

with:

```js
  document.querySelectorAll('[data-i18n-en]').forEach(element => {
    if (element.dataset.i18nUa === undefined) element.dataset.i18nUa = element.textContent;
    element.textContent = lang === 'en' ? element.dataset.i18nEn : element.dataset.i18nUa;
  });

  // EN-адреса посилання (Telegram з англійським готовим текстом): UA-адресу запам'ятовуємо, щоб повернути
  document.querySelectorAll('[data-i18n-href-en]').forEach(link => {
    if (link.dataset.i18nHrefUa === undefined) link.dataset.i18nHrefUa = link.getAttribute('href');
    link.setAttribute('href', lang === 'en' ? link.dataset.i18nHrefEn : link.dataset.i18nHrefUa);
  });
```

- [ ] **Step 5: Лайтбокс лише там, де він є**

Replace:

```js
document.addEventListener('DOMContentLoaded', () => {
  new GallerySlider();
  new Lightbox();
});
```

with:

```js
document.addEventListener('DOMContentLoaded', () => {
  new GallerySlider();
  // галерея з лайтбоксом є лише на головній
  if (document.getElementById('lightbox-modal')) new Lightbox();
});
```

- [ ] **Step 6: Перевірка на сторінці послуги**

`site.js` віддається як статичний файл, тому досить оновити сторінку з очищенням кешу (Ctrl+Shift+R) на `http://localhost:5080/farbuvannia`.

Expected: у консолі немає помилок.

У консолі:

```js
const a = document.createElement('a');
a.href = 'https://t.me/x?text=ua';
a.dataset.i18nHrefEn = 'https://t.me/x?text=en';
document.body.append(a);
switchLanguage('en');
const r1 = [document.querySelector('h1').textContent, a.getAttribute('href')];
switchLanguage('ua');
const r2 = [document.querySelector('h1').textContent, a.getAttribute('href')];
a.remove();
[r1, r2]
```

Expected: `[["Hair coloring", "https://t.me/x?text=en"], ["Фарбування волосся", "https://t.me/x?text=ua"]]`.

- [ ] **Step 7: Перевірка головної**

Відкрити `http://localhost:5080/` (Ctrl+Shift+R). У консолі не має бути помилок. Далі:

```js
document.getElementById('burger-btn').click();
const menuOpen = document.getElementById('mobile-menu').classList.contains('active');
document.getElementById('close-btn').click();
document.querySelector('.gallery-desktop-image, .gallery-mobile-image').click();
const lightboxOpen = document.getElementById('lightbox-modal').classList.contains('active');
document.getElementById('lightbox-close').click();
[menuOpen, lightboxOpen]
```

Expected: `[true, true]`. Перемикач EN/UA у меню перекладає головну як раніше.

- [ ] **Step 8: Commit**

```bash
git add ValentynaWeb/wwwroot/js/site.js
git commit -m "fix: site.js на сторінках без бургер-меню і лайтбокса; EN-адреси посилань (data-i18n-href-en)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: Розмітка і стилі сторінки (варіант Г)

Після задачі: сторінка виглядає як макет `docs/coloring-mockups/g-combined.html`.

Блоки:
- шапка;
- арка з фото;
- 8 робіт;
- шкала цін;
- 3 кроки;
- відгуки (поки лише посилання на Google);
- питання (поки одне — з відповіддю);
- фінал і контакти;
- нижня панель на телефоні.

Блок «Майстри» прихований: у `masters.json` ще ні в кого немає `coloring`.

**Files:**
- Modify: `ValentynaWeb/Content/Models/MastersContent.cs`
- Modify: `ValentynaWeb/Pages/Index.cshtml.cs` (`FormatPhone`, `ToScheduleLine` → `public`)
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml.cs` (повна заміна)
- Create: `ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml`
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml` (повна заміна)
- Create: `ValentynaWeb/wwwroot/css/service-page.css`

**Interfaces:**
- Consumes:
  - `PriceScale` (`Rows`, `Max`, `Ticks`, `Pct`, `Num`) — Task 2
  - `LocalizedText.EnAttr` — Task 1
  - `data-i18n-href-en` — Task 3
  - `IReviewsProvider.GetAsync() → ReviewsContent` (`Rating`, `ProfileUrl`)
  - `IndexModel.ScheduleLine(TranslateKey, Days, Time)`
  - SVG-символ `#fab-phone` з `_Layout.cshtml`
- Produces:
  - `Master.Services: List<string>`, `Master.About: LocalizedText?`
  - `IndexModel.FormatPhone(string) → string` (public static)
  - `IndexModel.ToScheduleLine(OpeningHours) → ScheduleLine` (public static)
  - ids секцій: `hero`, `works`, `prices`, `steps`, `masters`, `reviews`, `faq`, `contacts`, `contact-bar`

- [ ] **Step 1: Знімок «до»**

Застосунок на коді після Task 3. У браузері з шириною 375 px відкрити `http://localhost:5080/farbuvannia`, потім:

```js
[...document.querySelectorAll('main section[id]')].map(s => s.id).join(',')
```

Expected: `""` (порожньо — секцій ще немає).

- [ ] **Step 2: Поля майстра**

Modify `ValentynaWeb/Content/Models/MastersContent.cs`. Після рядка `    public string? Instagram { get; set; }` додати:

```csharp
    /// <summary>Id категорій прайсу, які робить майстер (напр. "coloring") — для блоку «Майстри» на сторінці послуги</summary>
    public List<string> Services { get; set; } = new();
    /// <summary>Кілька слів про досвід; ранг пишемо в Role, фото — в AvatarUrl</summary>
    public LocalizedText? About { get; set; }
```

- [ ] **Step 3: Публічні помічники головної**

Modify `ValentynaWeb/Pages/Index.cshtml.cs`:
- `    private static string FormatPhone(string phone) =>` → `    public static string FormatPhone(string phone) =>`
- `    private static ScheduleLine ToScheduleLine(OpeningHours h)` → `    public static ScheduleLine ToScheduleLine(OpeningHours h)`

- [ ] **Step 4: Модель сторінки — усі дані для розмітки**

Replace the whole `ValentynaWeb/Pages/ServicePage.cshtml.cs` with:

```csharp
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
```

- [ ] **Step 5: Шапка**

Create `ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml`:

```cshtml
@model ServicePageModel
@* Коротка шапка сторінок послуг: без бургер-меню — лише шлях на головну, мова і зв'язок.
   Телефон і «Надіслати фото» видно з 900px; на телефоні вони в нижній панелі (#contact-bar).
   Кнопки мови — ті самі .lang-btn[data-lang], що й на головній: їх обробляє switchLanguage у site.js *@
<header class="sp-topbar">
  <div class="sp-wrap sp-topbar-in">
    <a class="sp-logo" href="/" aria-label="Перукарня Валентина, на головну">
      <span data-translate="logo-main">ВАЛЕНТИНА</span>
      <span class="sp-logo-sub" data-translate="logo-sub">перукарня</span>
    </a>
    <nav class="sp-topnav" aria-label="Навігація">
      <a class="sp-topnav-link" href="/#price" data-i18n-en="All services">Усі послуги</a>
      <div class="sp-lang">
        <button type="button" class="lang-btn active" data-lang="ua">UA</button>
        <button type="button" class="lang-btn" data-lang="en">EN</button>
      </div>
      <a class="sp-topphone" href="tel:@Model.Salon.Phone">@Model.PhoneDisplay</a>
      <a class="sp-btn sp-btn-main sp-btn-sm sp-topcta" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
        <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
        <span data-i18n-en="Send a photo">Надіслати фото</span>
      </a>
    </nav>
  </div>
</header>
```

- [ ] **Step 6: Розмітка сторінки**

Replace the whole `ValentynaWeb/Pages/ServicePage.cshtml` with:

```cshtml
@page "/{slug}"
@model ServicePageModel
@{
    ViewData["PageTitle"] = Model.Service.Title;
    ViewData["Description"] = Model.Service.Description;
    // своя нижня панель «Надіслати фото» + дзвінок замість липкої «Записатися» (WLaunch) і круглої «Зв'язатися»
    ViewData["OwnContactBar"] = true;
    var s = Model.Service;
}
@section Styles {
    <link rel="stylesheet" href="~/css/service-page.css" asp-append-version="true" />
}
@section Head {
    <link rel="canonical" href="@Model.CanonicalUrl" />
    @if (!s.Published)
    {
        @* чернетка: відкривається за прямим посиланням, але Google її не індексує *@
        <meta name="robots" content="noindex" />
    }
    <script type="application/ld+json">@Html.Raw(Model.JsonLd)</script>
}

@* Значок Telegram кольором тексту (у спрайті _Layout він з fill="none"); телефон — #fab-phone з _Layout *@
<svg width="0" height="0" style="position:absolute" aria-hidden="true">
  <symbol id="sp-icon-tg" viewBox="0 0 18 18"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M17.9821 2.22666C18.0212 1.9646 17.996 1.69575 17.9091 1.44812C17.8222 1.2005 17.6769 0.98316 17.4882 0.818751C17.2996 0.654343 17.0745 0.548881 16.8364 0.513355C16.5983 0.477828 16.356 0.513535 16.1347 0.616758L0.821211 7.74547C-0.246253 8.2425 -0.30088 9.94943 0.821211 10.4643C1.93245 10.9762 3.05973 11.4445 4.20091 11.8682C5.24689 12.2504 6.38778 12.6039 7.30211 12.7039C7.55196 13.0346 7.86629 13.3534 8.18689 13.6445C8.67674 14.09 9.26599 14.5484 9.87495 14.988C11.0947 15.8692 12.4478 16.7118 13.3594 17.2603C14.4493 17.9138 15.7541 17.1613 15.9493 15.8474L17.9821 2.22666ZM2.53793 9.09497L16.0818 2.79003L14.1851 15.5029C13.2905 14.9653 12.0018 14.1603 10.8538 13.3306C10.3246 12.955 9.81378 12.5488 9.32331 12.1138C9.19223 11.9954 9.06498 11.8719 8.94181 11.7435L12.489 7.8227C12.657 7.63705 12.7515 7.3852 12.7516 7.12255C12.7516 6.85991 12.6573 6.60799 12.4894 6.4222C12.3215 6.23642 12.0937 6.132 11.8562 6.1319C11.6186 6.13181 11.3907 6.23606 11.2227 6.42171L7.33883 10.7158C6.68151 10.6227 5.76629 10.3534 4.76151 9.98705C4.01349 9.7115 3.27233 9.4137 2.53882 9.09398L2.53793 9.09497Z" /></symbol>
</svg>

<partial name="_ServicePageHeader" model="Model" />

<main class="sp-page">
  @* Перший екран: фото в арці (як дзеркало в салоні), на десктопі текст ліворуч *@
  <section class="sp-hero" id="hero">
    <div class="sp-wrap sp-hero-in">
      <div class="sp-arch">
        <responsive-img src="@s.HeroImage" alt="@s.HeroAlt" eager="true" sizes="(max-width: 899px) 100vw, 45vw" />
      </div>
      <div class="sp-hero-text">
        <h1 class="sp-display" data-i18n-en="@s.H1.EnAttr">@s.H1.Ua</h1>
        <p class="sp-lead" data-i18n-en="@s.Lead.EnAttr">@s.Lead.Ua</p>
        <div class="sp-btn-row">
          <a class="sp-btn sp-btn-main" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
            <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
            <span data-i18n-en="Send a photo on Telegram">Надіслати фото в Telegram</span>
          </a>
          @if (Model.Prices.Rows.Count > 0)
          {
            <a class="sp-btn sp-btn-line" href="#prices" data-i18n-en="Prices">Ціни</a>
          }
        </div>
        <p class="sp-facts">
          <span>@Model.Salon.Address</span>
          @foreach (var line in Model.Schedule)
          {
            <span><span data-translate="@line.TranslateKey">@line.Days</span> @line.Time</span>
          }
        </p>
      </div>
    </div>
  </section>

  @if (s.Works.Count > 0)
  {
    <section class="sp-block sp-works" id="works">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="Our work">Роботи</h2>
        <div class="sp-works-grid">
          @foreach (var work in s.Works)
          {
            <responsive-img src="@work" alt="@($"{s.H1.Ua} в Перукарні Валентина")" sizes="(max-width: 899px) 50vw, 25vw" />
          }
        </div>
        @if (Model.Salon.Socials.Instagram is { } instagram)
        {
          <a class="sp-more" href="@instagram" target="_blank" rel="noopener" data-i18n-en="More work on Instagram">Більше робіт в Instagram</a>
        }
      </div>
    </section>
  }

  @if (Model.Prices.Rows.Count > 0)
  {
    var lastTick = Model.Prices.Ticks.Count - 1;
    <section class="sp-block sp-block-sand" id="prices">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="Prices">Ціни</h2>
        <div class="sp-scale-box">
          <ul class="sp-scale">
            @if (Model.Prices.Ticks.Count > 0)
            {
              <li class="sp-scale-axis" aria-hidden="true">
                @for (var i = 0; i < Model.Prices.Ticks.Count; i++)
                {
                  var tick = Model.Prices.Ticks[i];
                  <span style="left:@Model.Prices.Pct(tick)%">@PriceScale.Num(tick)@(i == lastTick ? " грн" : "")</span>
                }
              </li>
            }
            @foreach (var row in Model.Prices.Rows)
            {
              <li>
                <div class="sp-scale-head">
                  <span data-translate="@row.TranslateKey">@row.Name</span>
                  <b>@row.PriceText</b>
                </div>
                @if (row.From is int lo && row.To is int hi)
                {
                  <div class="sp-scale-track" aria-hidden="true"><span class="sp-scale-fill" style="left:@Model.Prices.Pct(lo)%;width:@Model.Prices.Pct(hi - lo)%"></span></div>
                }
              </li>
            }
          </ul>
          @if (!string.IsNullOrWhiteSpace(s.PriceNote.Ua))
          {
            <p class="sp-price-note" data-i18n-en="@s.PriceNote.EnAttr">@s.PriceNote.Ua</p>
          }
        </div>
      </div>
    </section>
  }

  @if (s.Steps.Count > 0)
  {
    <section class="sp-block" id="steps">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="How it works">Як це працює</h2>
        <ol class="sp-steps">
          @for (var i = 0; i < s.Steps.Count; i++)
          {
            <li>
              <span class="sp-step-n" aria-hidden="true">@(i + 1)</span>
              <h3 data-i18n-en="@s.Steps[i].Title.EnAttr">@s.Steps[i].Title.Ua</h3>
              <p data-i18n-en="@s.Steps[i].Text.EnAttr">@s.Steps[i].Text.Ua</p>
            </li>
          }
        </ol>
      </div>
    </section>
  }

  @if (Model.Masters.Count > 0)
  {
    <section class="sp-block" id="masters">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="Stylists">Майстри</h2>
        <div class="sp-masters">
          @foreach (var m in Model.Masters)
          {
            <article class="sp-master">
              <img class="sp-master-photo" src="@m.AvatarUrl" alt="@m.Name" loading="lazy" />
              <h3>@m.Name</h3>
              <p class="sp-master-role">@m.Role</p>
              @if (m.About is { } about && !string.IsNullOrWhiteSpace(about.Ua))
              {
                <p data-i18n-en="@about.EnAttr">@about.Ua</p>
              }
            </article>
          }
        </div>
      </div>
    </section>
  }

  @if (s.Reviews.Count > 0 || Model.ReviewsUrl is not null)
  {
    <section class="sp-block sp-block-ink" id="reviews">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="Reviews">Відгуки</h2>
        @if (s.Reviews.Count > 0)
        {
          <div class="sp-quotes">
            @foreach (var review in s.Reviews)
            {
              <blockquote class="sp-quote">
                <p data-i18n-en="@review.Text.EnAttr">@review.Text.Ua</p>
                <footer>@review.Author</footer>
              </blockquote>
            }
          </div>
        }
        @if (Model.ReviewsUrl is { } reviewsUrl)
        {
          <a class="sp-rating-link" href="@reviewsUrl" target="_blank" rel="noopener">
            @if (Model.RatingText is { } rating)
            {
              <b>@rating</b>
            }
            <span data-i18n-en="All reviews on Google">Усі відгуки в Google</span>
          </a>
        }
      </div>
    </section>
  }

  @if (Model.Faq.Count > 0)
  {
    <section class="sp-block" id="faq">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="Questions">Питання</h2>
        <div class="sp-faq">
          @foreach (var item in Model.Faq)
          {
            <details>
              <summary data-i18n-en="@item.Question.EnAttr">@item.Question.Ua</summary>
              <p data-i18n-en="@item.Answer!.EnAttr">@item.Answer!.Ua</p>
            </details>
          }
        </div>
      </div>
    </section>
  }

  <section class="sp-block sp-block-sand" id="contacts">
    <div class="sp-wrap sp-final">
      <div>
        <h2 class="sp-title" data-i18n-en="Not sure what to choose?">Не знаєте, що обрати?</h2>
        <p class="sp-lead" data-i18n-en="Send a photo and a stylist will suggest what suits you.">Надішліть фото, і майстер підкаже, що підійде саме вам.</p>
        <div class="sp-btn-row">
          <a class="sp-btn sp-btn-main" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
            <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
            <span data-i18n-en="Send a photo on Telegram">Надіслати фото в Telegram</span>
          </a>
          <a class="sp-btn sp-btn-line" href="tel:@Model.Salon.Phone">
            <svg class="sp-icon" viewBox="0 0 24 24" aria-hidden="true"><use href="#fab-phone"></use></svg>
            <span data-translate="fab-call">Подзвонити</span>
          </a>
        </div>
      </div>
      <address class="sp-where">
        <b data-i18n-en="Valentyna hair salon">Перукарня Валентина</b>
        <span>@Model.Salon.Address</span>
        @foreach (var line in Model.Schedule)
        {
          <span><span data-translate="@line.TranslateKey">@line.Days</span> @line.Time</span>
        }
        <a href="tel:@Model.Salon.Phone">@Model.PhoneDisplay</a>
        @if (Model.Salon.Socials.GoogleMaps is { } maps)
        {
          <a href="@maps" target="_blank" rel="noopener" data-i18n-en="Get directions">Прокласти маршрут</a>
        }
        <a href="/#price" data-i18n-en="All salon services">Усі послуги перукарні</a>
      </address>
    </div>
  </section>
</main>

@* Нижня панель на телефоні (до 900px): головна дія завжди під рукою *@
<div class="sp-contact-bar" id="contact-bar">
  <a class="sp-btn sp-btn-main" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener" aria-label="Надіслати фото в Telegram">
    <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
    <span data-i18n-en="Send a photo">Надіслати фото</span>
  </a>
  <a class="sp-btn sp-btn-icon" href="tel:@Model.Salon.Phone" aria-label="Подзвонити">
    <svg class="sp-icon" viewBox="0 0 24 24" aria-hidden="true"><use href="#fab-phone"></use></svg>
  </a>
</div>
```

- [ ] **Step 7: Стилі**

Create `ValentynaWeb/wwwroot/css/service-page.css`:

```css
/* =====================================================
   Сторінки послуг (/farbuvannia …) — Pages/ServicePage.cshtml.
   Підключається лише там; усі класи з префіксом sp-, щоб не зачепити головну.
   Кольори, шрифти й кнопки — як на головній: Chutz для заголовків, Montserrat для тексту.
   Макет: docs/coloring-mockups/g-combined.html
   ===================================================== */

:root {
  --sp-ink: #1f1b19;
  --sp-paper: #fff;
  --sp-sand: #eee6dc;
  --sp-beige: #ddcdb3;
  --sp-rose: #ad776b;
  --sp-rose-dark: #8f5f55;
  --sp-smoke: #5a5a5a;
  --sp-line: #e3dccf;
  --sp-gutter: 16px;
  --sp-max: 1160px;
}

@media (min-width: 900px) {
  :root { --sp-gutter: 40px; }
}

.sp-topbar,
.sp-page,
.sp-contact-bar {
  font: 400 16px/1.55 'Montserrat', sans-serif;
  color: var(--sp-ink);
  -webkit-font-smoothing: antialiased;
}

.sp-page picture,
.sp-page img {
  display: block;
  max-width: 100%;
}

.sp-topbar :focus-visible,
.sp-page :focus-visible,
.sp-contact-bar :focus-visible {
  outline: 2px solid var(--sp-rose);
  outline-offset: 3px;
}

.sp-wrap {
  max-width: var(--sp-max);
  margin: 0 auto;
  padding: 0 var(--sp-gutter);
}

/* ---------- Типографіка ---------- */
.sp-display {
  font-family: 'Chutz', sans-serif;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  line-height: 1.02;
}

.sp-title {
  font-family: 'Chutz', sans-serif;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 30px;
  line-height: 1.1;
  letter-spacing: 0.05em;
  margin-bottom: 24px;
}

@media (min-width: 900px) {
  .sp-title { font-size: 44px; margin-bottom: 36px; }
}

.sp-lead {
  font-size: 18px;
  line-height: 1.5;
  max-width: 46ch;
}

/* ---------- Кнопки ---------- */
.sp-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  min-height: 54px;
  padding: 14px 24px;
  border: 1px solid transparent;
  border-radius: 10px;
  font: 500 17px/1.2 'Montserrat', sans-serif;
  text-decoration: none;
  cursor: pointer;
}

.sp-icon { width: 20px; height: 20px; flex: none; }

.sp-btn-main { background: var(--sp-rose-dark); color: #fff; }
.sp-btn-main:hover { background: #7a4f47; }
.sp-btn-line { border-color: currentColor; color: inherit; }
.sp-btn-line:hover { background: rgba(0, 0, 0, 0.04); }
.sp-btn-sm { min-height: 42px; padding: 9px 16px; font-size: 14px; }
.sp-btn-icon { width: 54px; padding: 0; border-color: var(--sp-ink); color: var(--sp-ink); }

.sp-btn-row { display: flex; flex-wrap: wrap; gap: 12px; }

/* ---------- Шапка ---------- */
.sp-topbar { background: var(--sp-ink); color: #fff; }

.sp-topbar-in {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  min-height: 68px;
}

.sp-logo {
  display: flex;
  flex-direction: column;
  font-family: 'Chutz', sans-serif;
  font-size: 20px;
  letter-spacing: 0.04em;
  line-height: 1;
  color: var(--sp-beige);
}

.sp-logo-sub {
  font-family: 'Roboto Mono', monospace;
  font-weight: 300;
  font-size: 18px;
}

.sp-topnav { display: flex; align-items: center; gap: 12px; font-size: 13px; }
.sp-topnav-link { color: #fff; opacity: 0.85; }
.sp-topnav-link:hover { opacity: 1; }

.sp-lang { display: flex; gap: 4px; }
.sp-lang .lang-btn { width: 40px; height: 36px; font-size: 13px; }

.sp-topphone,
.sp-topcta { display: none; }
.sp-topphone { color: #fff; font-weight: 500; white-space: nowrap; }

@media (min-width: 900px) {
  .sp-topnav { gap: 20px; font-size: 14px; }
  .sp-topphone,
  .sp-topcta { display: inline-flex; }
}

/* ---------- Перший екран ---------- */
.sp-hero { padding: 20px 0 8px; }
.sp-hero-in { display: grid; }

.sp-arch {
  width: 100%;
  max-width: 100%;
  aspect-ratio: 4 / 5;
  /* верх — півколо: горизонтальний радіус = 50% ширини, вертикальний = 40% висоти (4:5) */
  border-radius: 50% 50% 0 0 / 40% 40% 0 0;
  overflow: hidden;
  background: var(--sp-sand);
}

.sp-arch picture,
.sp-arch img { width: 100%; height: 100%; }
.sp-arch img { object-fit: cover; object-position: 50% 20%; }

.sp-hero-text { padding-top: 24px; }
.sp-hero h1 { font-size: 46px; margin-bottom: 18px; }
.sp-hero .sp-lead { margin-bottom: 24px; }
.sp-hero .sp-btn-row { margin-bottom: 20px; }
.sp-hero .sp-btn-main { flex: 1 1 260px; }

.sp-facts { display: grid; gap: 2px; font-size: 15px; color: var(--sp-smoke); }

@media (min-width: 900px) {
  .sp-hero { padding: 48px 0 24px; }
  .sp-hero-in { grid-template-columns: 1.1fr 1fr; align-items: end; gap: 56px; }
  .sp-hero-text { order: -1; padding: 0 0 40px; }
  .sp-hero h1 { font-size: 92px; }
  .sp-arch { max-height: 680px; }
  .sp-hero .sp-btn-main { flex: 0 0 auto; }
}

/* ---------- Секції ---------- */
.sp-block { padding: 56px 0; }
@media (min-width: 900px) { .sp-block { padding: 96px 0; } }
.sp-block-sand { background: var(--sp-sand); }
.sp-block-ink { background: var(--sp-ink); color: #fff; }

/* ---------- Роботи ---------- */
.sp-hero + .sp-works { padding-top: 32px; }

.sp-works-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
}

.sp-works-grid img {
  width: 100%;
  aspect-ratio: 4 / 5;
  object-fit: cover;
  border-radius: 4px;
}

@media (min-width: 900px) {
  .sp-works-grid { grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
}

.sp-more {
  display: inline-block;
  margin-top: 20px;
  font-weight: 500;
  color: var(--sp-rose-dark);
  text-decoration: underline;
  text-underline-offset: 3px;
}

/* ---------- Ціни на шкалі ---------- */
.sp-scale-box {
  max-width: 820px;
  background: var(--sp-paper);
  border-radius: 10px;
  padding: 20px 16px 4px;
}

@media (min-width: 900px) {
  .sp-scale-box { padding: 28px 32px 8px; }
}

.sp-scale { list-style: none; }
.sp-scale li { padding: 12px 0; }

/* Підписи осі стоять точно на своїх значеннях (0, 5 000, 10 000 з 13 000) */
.sp-scale .sp-scale-axis {
  position: relative;
  height: 26px;
  padding: 0;
  margin-bottom: 6px;
  font-size: 12px;
  color: var(--sp-smoke);
  border-bottom: 1px solid var(--sp-line);
}

.sp-scale-axis span { position: absolute; top: 0; transform: translateX(-50%); white-space: nowrap; }
.sp-scale-axis span:first-child { transform: none; }

.sp-scale-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 12px;
  margin-bottom: 8px;
  font-size: 15px;
}

.sp-scale-head b { font-weight: 600; white-space: nowrap; }

.sp-scale-track {
  position: relative;
  height: 10px;
  border-radius: 5px;
  background: rgba(31, 27, 25, 0.08);
}

.sp-scale-fill {
  position: absolute;
  top: 0;
  bottom: 0;
  min-width: 10px;
  border-radius: 5px;
  background: var(--sp-rose-dark);
}

.sp-price-note { margin: 8px 0 16px; max-width: 60ch; font-size: 14px; color: var(--sp-smoke); }

/* ---------- Як це працює ---------- */
.sp-steps { list-style: none; display: grid; gap: 28px; }

.sp-steps li { display: grid; grid-template-columns: 48px 1fr; gap: 4px 16px; }

.sp-step-n {
  grid-row: span 2;
  font-family: 'Chutz', sans-serif;
  font-size: 44px;
  line-height: 0.9;
  color: var(--sp-rose);
}

.sp-steps h3 { font-size: 18px; font-weight: 600; line-height: 1.3; }
.sp-steps p { color: var(--sp-smoke); max-width: 40ch; }

@media (min-width: 900px) {
  .sp-steps { grid-template-columns: repeat(3, 1fr); gap: 40px; }
}

/* ---------- Майстри ---------- */
.sp-masters { display: grid; gap: 24px; }

@media (min-width: 640px) {
  .sp-masters { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}

.sp-master-photo {
  width: 100%;
  aspect-ratio: 4 / 5;
  object-fit: cover;
  border-radius: 4px;
  margin-bottom: 14px;
}

.sp-master h3 { font-size: 18px; font-weight: 600; }
.sp-master p { font-size: 15px; color: var(--sp-smoke); }

/* ---------- Відгуки ---------- */
.sp-quotes { display: grid; gap: 32px; margin-bottom: 36px; }

@media (min-width: 900px) {
  .sp-quotes { grid-template-columns: repeat(2, 1fr); gap: 56px; }
}

.sp-quote p { font-size: 21px; font-weight: 300; line-height: 1.45; max-width: 36ch; }
.sp-quote p::before { content: '«'; }
.sp-quote p::after { content: '»'; }
.sp-quote footer { margin-top: 12px; font-size: 14px; color: var(--sp-beige); }

.sp-rating-link {
  display: inline-flex;
  gap: 8px;
  align-items: baseline;
  color: #fff;
  font-weight: 500;
  text-decoration: underline;
  text-underline-offset: 3px;
}

.sp-rating-link b { font-family: 'Chutz', sans-serif; font-size: 26px; color: var(--sp-beige); }

/* ---------- Питання ---------- */
.sp-faq { max-width: 760px; }
.sp-faq details { border-bottom: 1px solid var(--sp-line); }

.sp-faq summary {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 18px 0;
  font-weight: 600;
  font-size: 17px;
  cursor: pointer;
  list-style: none;
}

.sp-faq summary::-webkit-details-marker { display: none; }

.sp-faq summary::after {
  content: '+';
  font-weight: 400;
  font-size: 24px;
  line-height: 1;
  transition: transform 0.2s ease;
}

.sp-faq details[open] summary::after { transform: rotate(45deg); }
.sp-faq details p { padding: 0 0 20px; max-width: 60ch; color: var(--sp-smoke); }

/* ---------- Фінал і контакти ---------- */
.sp-final { display: grid; gap: 40px; }

@media (min-width: 900px) {
  .sp-final { grid-template-columns: 1.2fr 1fr; align-items: end; }
}

.sp-final .sp-title { margin-bottom: 16px; }
.sp-final .sp-lead { margin-bottom: 24px; }

.sp-where { font-style: normal; display: grid; gap: 6px; font-size: 16px; }
.sp-where a { font-weight: 500; text-decoration: underline; text-underline-offset: 3px; }

/* ---------- Нижня панель на телефоні ---------- */
.sp-contact-bar {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
  display: flex;
  gap: 8px;
  padding: 10px var(--sp-gutter) calc(10px + env(safe-area-inset-bottom));
  background: rgba(255, 255, 255, 0.96);
  border-top: 1px solid var(--sp-line);
}

.sp-contact-bar .sp-btn-main { flex: 1; }

/* body.has-contact-bar ставить _Layout (ViewData["OwnContactBar"]): місце під панеллю,
   кругла «Зв'язатися» на телефоні не потрібна — дзвінок і Telegram уже в панелі */
@media (max-width: 899px) {
  .has-contact-bar { padding-bottom: calc(76px + env(safe-area-inset-bottom)); }
  .has-contact-bar .contact-fab { display: none; }
}

@media (min-width: 900px) {
  .sp-contact-bar { display: none; }
}

@media (prefers-reduced-motion: reduce) {
  .sp-faq summary::after { transition: none; }
}
```

- [ ] **Step 8: Збірка**

Run: `dotnet build ValentynaWeb/ValentynaWeb.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 9: Перевірка на телефоні (375 px)**

Перезапустити застосунок. Відкрити `http://localhost:5080/farbuvannia` у браузері з шириною 375 px. Консоль має бути без помилок. Далі:

```js
({
  overflow: document.documentElement.scrollWidth - innerWidth,
  sections: [...document.querySelectorAll('main section[id]')].map(s => s.id).join(','),
  rows: document.querySelectorAll('.sp-scale li:not(.sp-scale-axis)').length,
  axis: [...document.querySelectorAll('.sp-scale-axis span')].map(s => s.style.left).join(' '),
  firstRow: document.querySelector('.sp-scale-head span').textContent,
  tg: decodeURIComponent(document.querySelector('#hero a[href*="t.me"]').href),
  bar: getComputedStyle(document.getElementById('contact-bar')).display,
  fab: getComputedStyle(document.getElementById('contact-fab')).display,
  stickyBook: document.querySelectorAll('.sticky-book-btn').length,
  h1: document.querySelectorAll('h1').length
})
```

Expected:
```
overflow: 0
sections: "hero,works,prices,steps,reviews,faq,contacts"
rows: 7
axis: "0% 38.46% 76.92%"
firstRow: "Тонування"
tg: "https://t.me/valentyna_perukarnia?text=Привіт! Пишу з сайту щодо фарбування."
bar: "flex"
fab: "none"
stickyBook: 0
h1: 1
```

Прокрутити донизу: нижня панель не закриває кнопки й адресу в блоці `#contacts`.

- [ ] **Step 10: Перевірка мови й аналітики**

На тій самій сторінці:

```js
document.querySelector('.lang-btn[data-lang="en"]').click();
const en = {
  h1: document.querySelector('h1').textContent,
  tg: decodeURIComponent(document.querySelector('#contact-bar a[href*="t.me"]').href),
  row: document.querySelector('.sp-scale-head span').textContent,
  step: document.querySelector('#steps h3').textContent
};
document.querySelector('.lang-btn[data-lang="ua"]').click();
en
```

Expected:
- `h1: "Hair coloring"`;
- `tg` закінчується на `?text=Hi! I'm writing from your website about hair coloring.`;
- `row: "Toning"`;
- `step: "Send a photo of your hair on Telegram"`.

```js
window.dataLayer = window.dataLayer || [];
const pick = sel => { const a = document.querySelector(sel); a.addEventListener('click', e => e.preventDefault(), { once: true }); a.click(); const e = window.dataLayer.at(-1); return [e.event, e.link_location]; };
[pick('#contact-bar a[href*="t.me"]'), pick('#contacts a[href^="tel:"]'), pick('#hero a[href*="t.me"]')]
```

Expected: `[["telegram_click","contact-bar"],["phone_click","contacts"],["telegram_click","hero"]]`.

- [ ] **Step 11: Перевірка на десктопі (1440 px) і звірка з макетом**

Ширина 1440 px:
- шапка з телефоном і «Надіслати фото»;
- текст першого екрана ліворуч, арка праворуч;
- роботи в 4 колонки;
- кроки в 3 колонки;
- нижньої панелі немає, кругла «Зв'язатися» є.

Порівняти скриншоти 375 і 1440 з `docs/coloring-mockups/g-combined.html` (`node docs/coloring-mockups/serve.js . 5091` → `http://localhost:5091/docs/coloring-mockups/g-combined.html`). Відмінності, які мають бути:
- немає блоку «Майстри» і цитат (немає даних);
- у питаннях одне питання.

- [ ] **Step 12: Commit**

```bash
git add ValentynaWeb/Content/Models/MastersContent.cs ValentynaWeb/Pages/Index.cshtml.cs ValentynaWeb/Pages/ServicePage.cshtml ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml ValentynaWeb/wwwroot/css/service-page.css
git commit -m "feat: сторінка «Фарбування» — роботи, шкала цін, кроки, відгуки, питання, нижня панель Telegram" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 5: Посилання з головної (лише для опублікованих сторінок)

Після задачі:
- поки `published: false`, головна без змін;
- з `published: true` у картці прайсу «Фарбування» є «Детальніше про фарбування»;
- з `published: true` у сторіс «Фарбування» кнопка «Ціни та запис» веде на `/farbuvannia`.

**Files:**
- Modify: `ValentynaWeb/Content/Models/ServicePhotosContent.cs`
- Modify: `ValentynaWeb/Content/service-photos.json`
- Modify: `ValentynaWeb/Pages/Index.cshtml.cs`
- Modify: `ValentynaWeb/Pages/Index.cshtml` (картка прайсу)
- Modify: `ValentynaWeb/Pages/Shared/_ServiceStories.cshtml`
- Modify: `ValentynaWeb/wwwroot/js/service-stories.js`
- Modify: `ValentynaWeb/wwwroot/css/site.css` (додати в кінець)

**Interfaces:**
- Consumes:
  - `ServicePagesContent.FindPublishedByCategory` (Task 1)
  - `ServicePage.Slug`, `ServicePage.CardLinkText`
  - ключ перекладу `stories-page` (Task 3)
- Produces:
  - `IndexModel.PublishedPageFor(string? categoryId) → ServicePage?`
  - `ServicePhotoCategory.ServiceId`
  - поле `pageUrl` у `#service-stories-data`

- [ ] **Step 1: Перевірка «до»**

Застосунок на коді після Task 4:

```bash
BASE=http://localhost:5080
curl -s $BASE/ | grep -c 'pageUrl'
```

Expected: `0`.

- [ ] **Step 2: Зв'язок категорії сторіс з прайсом**

Modify `ValentynaWeb/Content/Models/ServicePhotosContent.cs`. Після `    public string? TitleKey { get; set; }` додати:

```csharp
    /// <summary>Id категорії прайсу (services.json), напр. "coloring": якщо в неї є опублікована сторінка, кнопка в сторіс веде туди</summary>
    public string? ServiceId { get; set; }
```

Modify `ValentynaWeb/Content/service-photos.json`. Replace `      "titleKey": "services-coloring",` with:

```json
      "titleKey": "services-coloring",
      "serviceId": "coloring",
```

- [ ] **Step 3: Пошук сторінки з головної**

Modify `ValentynaWeb/Pages/Index.cshtml.cs`.

Після `    private readonly IReviewsProvider _reviews;` додати:

```csharp
    private ServicePagesContent _servicePages = new();
```

Replace `        ServicePhotos = _content.ServicePhotos.Categories;` with:

```csharp
        ServicePhotos = _content.ServicePhotos.Categories;
        _servicePages = _content.ServicePages;
```

Після `    public IReadOnlyList<ServicePhotoCategory> ServicePhotos { get; private set; } = [];` додати:

```csharp
    /// <summary>Опублікована сторінка послуги для категорії (id з services.json) — для посилань з картки прайсу і сторіс; чернетки не показуємо</summary>
    public ServicePage? PublishedPageFor(string? categoryId) => _servicePages.FindPublishedByCategory(categoryId);
```

- [ ] **Step 4: Посилання в картці прайсу**

Modify `ValentynaWeb/Pages/Index.cshtml`. Replace:

```cshtml
          </ul>
        </div>
      }
    </div>
    <div class="scroller-dots price-dots" aria-hidden="true"></div>
```

with:

```cshtml
          </ul>
          @if (Model.PublishedPageFor(category.Id) is { } servicePage)
          {
            <a href="/@servicePage.Slug" class="price-more" data-i18n-en="@servicePage.CardLinkText.EnAttr">@servicePage.CardLinkText.Ua</a>
          }
        </div>
      }
    </div>
    <div class="scroller-dots price-dots" aria-hidden="true"></div>
```

Додати в кінець `ValentynaWeb/wwwroot/css/site.css`:

```css

/* Посилання на сторінку послуги внизу картки прайсу («Детальніше про фарбування»).
   Є лише в категорій з опублікованою сторінкою в service-pages.json */
.price-more {
  position: relative;
  z-index: 2;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 48px;
  margin-top: 32px;
  padding: 10px 16px;
  border: 1px solid #ddcdb3;
  border-radius: 10px;
  color: #ddcdb3;
  font-family: 'Montserrat', sans-serif;
  font-size: 15px;
  font-weight: 500;
  text-align: center;
  transition: background 0.3s ease;
}

.price-more:hover,
.price-more:focus-visible {
  background: rgba(221, 205, 179, 0.1);
}
```

- [ ] **Step 5: Адреса сторінки в даних сторіс**

Modify `ValentynaWeb/Pages/Shared/_ServiceStories.cshtml`. Replace:

```cshtml
        photos = c.Photos.Select(IndexModel.WebpBase)
    });
```

with:

```cshtml
        photos = c.Photos.Select(IndexModel.WebpBase),
        // своя опублікована сторінка послуги (фарбування): кнопка внизу сторіс веде туди замість WLaunch
        pageUrl = Model.PublishedPageFor(c.ServiceId) is { } servicePage ? "/" + servicePage.Slug : null
    });
```

- [ ] **Step 6: Кнопка в сторіс**

Modify `ValentynaWeb/wwwroot/js/service-stories.js`. Replace:

```js
  const nextCatBtn = $('ss-next-cat');
```

with:

```js
  const nextCatBtn = $('ss-next-cat');
  const bookBtn = dialog.querySelector('.ss-book');
  const bookingUrl = bookBtn.getAttribute('href');
```

Replace:

```js
    if (nextCat) nextCatBtn.textContent = `${t('stories-next', 'Далі')}: ${titleOf(nextCat)} ›`;
```

with:

```js
    if (nextCat) nextCatBtn.textContent = `${t('stories-next', 'Далі')}: ${titleOf(nextCat)} ›`;

    // Категорія зі своєю сторінкою (фарбування) веде туди: там ціни й запис через Telegram.
    // Решта — в онлайн-запис WLaunch, як і було. data-translate міняємо разом із текстом,
    // щоб перемикач мови в site.js підставляв правильний підпис
    if (c.pageUrl) {
      bookBtn.href = c.pageUrl;
      bookBtn.removeAttribute('target');
      bookBtn.dataset.translate = 'stories-page';
      bookBtn.textContent = t('stories-page', 'Ціни та запис');
    } else {
      bookBtn.href = bookingUrl;
      bookBtn.target = '_blank';
      bookBtn.dataset.translate = 'hero-book';
      bookBtn.textContent = t('hero-book', 'Записатися');
    }
```

- [ ] **Step 7: Збірка**

Run: `dotnet build ValentynaWeb/ValentynaWeb.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 8: Перевірка з чернеткою — головна без змін**

Перезапустити застосунок:

```bash
BASE=http://localhost:5080
curl -s $BASE/ | grep -c 'class="price-more"'
curl -s $BASE/ | grep -o '"pageUrl":[^,}]*' | sort | uniq -c
```

Expected: `0`, потім `      6 "pageUrl":null`.

- [ ] **Step 9: Перевірка з тимчасовою публікацією**

Тимчасово поставити `"published": true` у `ValentynaWeb/Content/service-pages.json`, перезапустити застосунок:

```bash
BASE=http://localhost:5080
curl -s $BASE/ | grep -o '<a href="/farbuvannia" class="price-more"'
curl -s $BASE/ | grep -o '"pageUrl":"/farbuvannia"'
curl -s $BASE/sitemap.xml | grep -o '<loc>[^<]*</loc>'
curl -s $BASE/farbuvannia | grep -c 'noindex'
```

Expected:
```
<a href="/farbuvannia" class="price-more"
"pageUrl":"/farbuvannia"
<loc>https://localhost:5080/</loc>
<loc>https://localhost:5080/farbuvannia</loc>
0
```

У браузері на `http://localhost:5080/` (375 px):
- у картці «Фарбування» внизу кнопка «Детальніше про фарбування», в інших картках її немає;
- відкрити сторіс «Фарбування» (картка в секції «Послуги»): кнопка внизу «Ціни та запис», href `/farbuvannia`, без `target`;
- натиснути «Далі: Брови ›»: кнопка знову «Записатися» з адресою WLaunch і `target="_blank"`;
- перемкнути на EN і знову відкрити «Фарбування»: «Prices & booking»; картка прайсу: «More about hair coloring».

- [ ] **Step 10: Повернути чернетку**

Повернути `"published": false` у `service-pages.json`. Потім:

```bash
git diff --stat -- ValentynaWeb/Content/service-pages.json
```

Expected: порожній вивід (файл не змінено відносно коміту).

- [ ] **Step 11: Commit**

```bash
git add ValentynaWeb/Content/Models/ServicePhotosContent.cs ValentynaWeb/Content/service-photos.json ValentynaWeb/Pages/Index.cshtml ValentynaWeb/Pages/Index.cshtml.cs ValentynaWeb/Pages/Shared/_ServiceStories.cshtml ValentynaWeb/wwwroot/js/service-stories.js ValentynaWeb/wwwroot/css/site.css
git commit -m "feat: посилання на опубліковані сторінки послуг з картки прайсу і сторіс" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 6: Повна перевірка за спекою і PR

**Files:** без змін у коді (лише виправлення, якщо перевірка щось знайде; кожне — окремим `fix:`-комітом).

- [ ] **Step 1: Адреси й head**

Перезапустити застосунок (`published: false`):

```bash
BASE=http://localhost:5080
for p in farbuvannia FARBUVANNIA nema-takoi robots.txt sitemap.xml health ""; do printf '%-12s %s\n' "/$p" "$(curl -s -o /dev/null -w '%{http_code}' $BASE/$p)"; done
curl -s $BASE/farbuvannia | grep -c 'noindex'
curl -s $BASE/farbuvannia | grep -o '<link rel="canonical"[^>]*>'
curl -s $BASE/farbuvannia | grep -c '<h1'
curl -s $BASE/sitemap.xml | grep -c '<loc>'
curl -s $BASE/ | grep -c -E 'class="price-more"|"pageUrl":"/'
```

Expected:
- статуси як у Task 1 Step 10;
- далі: `1`, `<link rel="canonical" href="https://localhost:5080/farbuvannia" />`, `1`, `1`, `0`.

- [ ] **Step 2: Браузер**

Повторити перевірки з Task 3 Step 7 (головна), Task 4 Steps 9–11 (сторінка на 375 і 1440 px, мова, аналітика). Вибір мови має зберігатися при переході між `/` і `/farbuvannia` (`localStorage 'preferred-language'`).

- [ ] **Step 3: Зупинитися і спитати користувача**

Показати користувачу підсумок перевірок і скриншоти 375/1440. Спитати, чи пушити гілку і відкривати PR у `main`. Без його «так» нічого не пушити.

- [ ] **Step 4: Push і PR (лише після згоди)**

```bash
git push -u origin feat/coloring-page
gh pr create --base main --head feat/coloring-page --title "feat: сторінка «Фарбування» (чернеткою)" --body "$(cat <<'EOF'
Сторінка /farbuvannia під рекламу Google Ads за спекою docs/superpowers/specs/2026-11-07-coloring-page-design.md.

- Сторінка-шаблон /{slug}, контент у Content/service-pages.json, ціни з services.json.
- Публікується чернеткою (published: false): відкривається за прямим посиланням, з noindex, без sitemap і без посилань з головної. Головна не змінюється.
- Публікація для всіх — окремий коміт: published: true у service-pages.json.
- site.js більше не падає на сторінках без бургер-меню і лайтбокса.

Після деплою перевірити вручну:
- готовий текст у Telegram (t.me/valentyna_perukarnia?text=…) на iPhone, Android і Telegram Desktop;
- JSON-LD у Google Rich Results Test.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

- [ ] **Step 5: Після деплою (руками, з командою)**

1. **Telegram.** Відкрити `https://valentynah.com/farbuvannia` на iPhone, Android і в Telegram Desktop, натиснути «Надіслати фото в Telegram». Очікування: чат `valentyna_perukarnia` з уже набраним «Привіт! Пишу з сайту щодо фарбування.». Якщо тексту немає, прибрати `telegramText` у `service-pages.json` (окремий `fix:`-коміт).
2. **JSON-LD.** https://search.google.com/test/rich-results для `https://valentynah.com/farbuvannia`. Очікування: без помилок.
3. **Показ партнерам** за прямим посиланням. Правки — звичайними комітами, `published` лишається `false`.
