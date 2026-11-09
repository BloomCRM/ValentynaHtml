# Переходи на сторінки послуг — план реалізації

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Посилання на сторінки послуг з меню головної (десктоп і бургер), з карток «Послуг», з підвалу і між самими сторінками; на живому сайті — лише на опубліковані, локально — на всі.

**Architecture:** Новий клас `ServicePageLinks` (singleton) вирішує, на які сторінки посилатися (опубліковані; у Development — усі), у якому порядку (як картки «Послуг») і з якою обкладинкою. Головна і сторінки послуг беруть список у нього; наявні посилання (картка прайсу, кнопка в сторіз) переходять на той самий список.

**Tech Stack:** ASP.NET Core 8 Razor Pages, JSON-контент (`IContentProvider`), CSS без збирача. Тестового проєкту немає — перевірка Node-скриптами на запущеному сайті.

**Спека:** `docs/superpowers/specs/2026-11-09-service-pages-nav-design.md`

## Global Constraints

- Гілка `feat/service-pages`. Не пушити, не відкривати PR, не зливати в `main`.
- У кожному коміті всі сторінки в `service-pages.json` мають `"published": false`; цей файл у задачах не змінюється.
- **Production-режим не змінюється, крім трьох речей:** пункт «Послуги» в меню на десктопі (без списку), адреси «Швидких посилань» у підвалі, `id="reviews"` на блоці відгуків. Перевірка — `compare-prod.mjs` (задача 1): `home: SAME`, `farbuvannia: SAME`.
- Видимість: посилання — на `Published` сторінки, а в Development (`IHostEnvironment.IsDevelopment()`) — на всі. `noindex`, sitemap, JSON-LD від цього не залежать.
- Порядок сторінок у всіх списках — як картки в `service-photos.json` (поле `page`); назва — `ServicePage.H1` (UA у розмітці, EN через `data-i18n-en`).
- Процес на порту 5080 не чіпати. Процеси зупиняти лише за PID і лише свої. Свій сервер — порт 5190.
- Конвенції: C# — PascalCase, коментарі українською; `*.cs`, `*.cshtml`, `*.css` — CRLF (редагуйте інструментом Edit); `site.js` — UTF-8 з BOM; змінна Razor не може називатися `section` (директива).
- Коміти: `feat:`/`fix:` українською, в кінці — `Co-Authored-By:` моделі, що робила коміт.

## Як запускати сайт для перевірки

З кореня репозиторію, PowerShell. **Development** (видно чернетки):

```powershell
dotnet build ValentynaWeb -nologo -v q
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$app = Start-Process dotnet -ArgumentList 'bin/Debug/net8.0/ValentynaWeb.dll','--urls','http://localhost:5190' -WorkingDirectory 'C:\Users\Vova 2nd\Projects\ValentynaHtml\ValentynaWeb' -PassThru -WindowStyle Hidden
$app.Id
```

**Production** (як живий сайт; без GTM, щоб не слати аналітику з локальної машини і щоб HTML можна було порівняти з еталоном):

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:Analytics__GtmContainerId = ' '   # пробіл: порожній рядок у PowerShell видаляє змінну, і GTM повернеться з appsettings
$app = Start-Process dotnet -ArgumentList 'bin/Debug/net8.0/ValentynaWeb.dll','--urls','http://localhost:5190' -WorkingDirectory 'C:\Users\Vova 2nd\Projects\ValentynaHtml\ValentynaWeb' -PassThru -WindowStyle Hidden
$app.Id
```

Готовність: `curl -s -o NUL -w "%{http_code}" http://localhost:5190/health` → `200` (2–5 с). Зупинка: `Stop-Process -Id <Id>`; перед збіркою сервер зупинити (dll заблоковані). Після Production-перевірки поверніть змінні: `Remove-Item Env:Analytics__GtmContainerId; $env:ASPNETCORE_ENVIRONMENT = 'Development'`. Якщо `node` падає з `libuv` assertion після виводу — це відомий збій, перезапустіть.

---

### Task 1: Хто і куди посилається — `ServicePageLinks`

**Files:**
- Create: `ValentynaWeb/Pages/ServicePageLinks.cs`
- Modify: `ValentynaWeb/Program.cs` (реєстрація)
- Modify: `ValentynaWeb/Pages/Index.cshtml.cs`
- Modify: `ValentynaWeb/Pages/Index.cshtml` (посилання в картці прайсу)
- Modify: `ValentynaWeb/Content/Models/ServicePagesContent.cs` (прибрати методи, що стали зайвими)
- Create (не в git): `.superpowers/sdd/check-nav.mjs`, `.superpowers/sdd/compare-prod.mjs`

**Interfaces:**
- Produces: `ServicePageLinks` (singleton) з `IReadOnlyList<ServicePageLinks.Link> All()`; `record Link(ServicePage Page, string Cover, bool HasPrices)`. `IndexModel.ServiceLinks` (`IReadOnlyList<ServicePageLinks.Link>`), `IndexModel.ServiceLinksFor(string? categoryId)`, `IndexModel.ServiceLinkFor(string? pageSlug)` → `Link?`, `IndexModel.LinkLabel(Link)` → `(string Key, string Text)`.
- Видаляються: `ServicePagesContent.FindPublished`, `ServicePagesContent.PublishedForCategory`, `IndexModel.PublishedPagesFor`, `IndexModel.HasPrices`, поле `IndexModel._servicePages`.

- [ ] **Step 1: Скрипти перевірки**

`.superpowers/sdd/check-nav.mjs`:

```js
// Посилання на сторінки послуг: меню, бургер, плашки карток, підвал, прайс, сторіз, «Інші послуги».
// node .superpowers/sdd/check-nav.mjs [http://localhost:5190]
const base = process.argv[2] ?? 'http://localhost:5190';
const get = async p => (await fetch(`${base}${p}?nav=${Date.now()}`)).text();
const hrefs = (html, cls) => [...html.matchAll(new RegExp(`<a[^>]*class="${cls}"[^>]*>`, 'g'))]
  .map(m => (m[0].match(/href="([^"]*)"/) ?? [])[1]);
const show = list => list.length ? list.join(' ') : '-';

const home = await get('/');
console.log('desktop menu:', show(hrefs(home, 'nav-dropdown-link')));
console.log('burger menu: ', show(hrefs(home, 'mobile-nav-link mobile-nav-sublink')));
console.log('card pills:  ', show(hrefs(home, 'services-page-link')));
console.log('footer:      ', show(hrefs(home, 'footer-link footer-service-link')));
console.log('footer quick:', show(hrefs(home, 'footer-link')));
console.log('price cards: ', show(hrefs(home, 'price-more')));
const stories = JSON.parse(home.match(/id="service-stories-data">([\s\S]*?)<\/script>/)[1]);
console.log('stories:     ', stories.map(c => `${c.cta.key}:${c.cta.url.startsWith('/') ? c.cta.url : 'WLaunch'}`).join(' '));
for (const slug of ['farbuvannia', 'stryzhky', 'ukladky', 'dohliad', 'zavyvka', 'brovy']) {
  console.log(`others /${slug}:`.padEnd(20), show(hrefs(await get('/' + slug), 'sp-other')));
}
```

`.superpowers/sdd/compare-prod.mjs`:

```js
// Production-режим: HTML головної і «Фарбування» проти еталону (.superpowers/sdd/baseline/*.html), без трьох очікуваних змін:
// пункт «Послуги» в меню на десктопі, адреси «Швидких посилань» у підвалі, id="reviews". Решта має збігтися.
// node .superpowers/sdd/compare-prod.mjs [http://localhost:5190]
import { readFileSync } from 'node:fs';

const base = process.argv[2] ?? 'http://localhost:5190';
const norm = s => s
  .replace(/localhost:\d+/g, 'localhost')
  .replace(/\?v=[\w-]+/g, '')
  .replace(/<li class="nav-item-services">[\s\S]*?<\/li>/, '')
  .replace(/href="#(home|about|price|gallery|reviews)" class="footer-link"/g, 'href="#" class="footer-link"')
  .replace(/ id="reviews"/g, '')
  .replace(/>\s+</g, '><')
  .replace(/\s+/g, ' ')
  .trim();
let failed = false;

for (const [name, path] of [['home', '/'], ['farbuvannia', '/farbuvannia']]) {
  const expected = norm(readFileSync(new URL(`./baseline/${name}.html`, import.meta.url), 'utf8'));
  const actual = norm(await (await fetch(`${base}${path}?cmp=${Date.now()}`)).text());
  if (expected === actual) {
    console.log(`${name}: SAME`);
    continue;
  }
  failed = true;
  let i = 0;
  while (expected[i] === actual[i]) i++;
  console.log(`${name}: DIFF at ${i}`);
  console.log(`  expected: …${expected.slice(Math.max(0, i - 80), i + 160)}`);
  console.log(`  actual:   …${actual.slice(Math.max(0, i - 80), i + 160)}`);
}
process.exit(failed ? 1 : 0);
```

Еталон у `.superpowers/sdd/baseline/` знятий у Development до гілки; GTM у Production вимкнений змінною (див. «Як запускати»), тож інших відмінностей бути не повинно. Якщо єдина відмінність — текст плашки найближчої події («Сьогодні» / дата), це не регресія: напишіть у звіті.

- [ ] **Step 2: `ServicePageLinks`**

Створіть `ValentynaWeb/Pages/ServicePageLinks.cs`:

```csharp
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
```

У `ValentynaWeb/Program.cs` після рядка `builder.Services.AddSingleton<IContentProvider, JsonContentProvider>();` додайте:

```csharp
builder.Services.AddSingleton<ValentynaWeb.Pages.ServicePageLinks>();
```

(Якщо в `Program.cs` уже є `using ValentynaWeb.Pages;` — можна без префікса.)

- [ ] **Step 3: Головна бере посилання з `ServicePageLinks`**

У `ValentynaWeb/Pages/Index.cshtml.cs`:

Поле і конструктор — замініть:

```csharp
    private readonly IReviewsProvider _reviews;
    private ServicePagesContent _servicePages = new();

    public IndexModel(IConfiguration config, IContentProvider content, IReviewsProvider reviews)
    {
        _config = config;
        _content = content;
        _reviews = reviews;
    }
```

на:

```csharp
    private readonly IReviewsProvider _reviews;
    private readonly ServicePageLinks _links;

    public IndexModel(IConfiguration config, IContentProvider content, IReviewsProvider reviews, ServicePageLinks links)
    {
        _config = config;
        _content = content;
        _reviews = reviews;
        _links = links;
    }
```

Замініть:

```csharp
    /// <summary>Опубліковані сторінки послуг категорії прайсу (id з services.json) — посилання внизу картки; чернетки не показуємо</summary>
    public IReadOnlyList<ServicePage> PublishedPagesFor(string? categoryId) => _servicePages.PublishedForCategory(categoryId);
```

на:

```csharp
    /// <summary>Сторінки послуг для посилань (меню, картки, підвал, прайс, сторіз): опубліковані; локально — усі</summary>
    public IReadOnlyList<ServicePageLinks.Link> ServiceLinks { get; private set; } = [];

    /// <summary>Сторінки категорії прайсу (id з services.json) — посилання внизу картки прайсу; в «Миття, укладки та догляд» їх дві</summary>
    public IEnumerable<ServicePageLinks.Link> ServiceLinksFor(string? categoryId) =>
        string.IsNullOrEmpty(categoryId)
            ? []
            : ServiceLinks.Where(l => string.Equals(l.Page.ServiceCategoryId, categoryId, StringComparison.Ordinal));

    /// <summary>Сторінка картки «Послуг» (поле page у service-photos.json); null — посилатися нікуди</summary>
    public ServicePageLinks.Link? ServiceLinkFor(string? pageSlug) =>
        ServiceLinks.FirstOrDefault(l => string.Equals(l.Page.Slug, pageSlug, StringComparison.OrdinalIgnoreCase));

    /// <summary>Підпис посилання на сторінку і його ключ перекладу: «Ціни та запис», якщо на сторінці є ціни, інакше «Детальніше»</summary>
    public static (string Key, string Text) LinkLabel(ServicePageLinks.Link link) =>
        link.HasPrices ? ("stories-page", "Ціни та запис") : ("stories-more", "Детальніше");
```

В `OnGetAsync` замініть:

```csharp
        ServicePhotos = _content.ServicePhotos.Categories;
        _servicePages = _content.ServicePages;
```

на:

```csharp
        ServicePhotos = _content.ServicePhotos.Categories;
        ServiceLinks = _links.All();
```

і замініть:

```csharp
        BookingUrl = BookingLink.From(_config);
        // прайс — до сторіз: кнопка в сторіз залежить від того, чи є на сторінці послуги ціни
        ServiceCategories = _content.Services.Categories;
        Stories = new ServiceStories(ServicePhotos.Select(c => new ServiceStories.Category(
            c.Title, c.TitleKey, null, c.Cover, c.Photos, StoryButton(c.Page))).ToList());
```

на:

```csharp
        BookingUrl = BookingLink.From(_config);
        ServiceCategories = _content.Services.Categories;
        Stories = new ServiceStories(ServicePhotos.Select(c => new ServiceStories.Category(
            c.Title, c.TitleKey, null, c.Cover, c.Photos, StoryButton(c.Page))).ToList());
```

Замініть метод `StoryButton` і метод `HasPrices` (разом із коментарями над ними):

```csharp
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
```

на:

```csharp
    // Кнопка внизу сторіз: картка зі сторінкою, на яку можна посилатися, веде туди (підпис — LinkLabel);
    // решта — в онлайн-запис WLaunch
    private ServiceStories.Cta StoryButton(string? pageSlug)
    {
        if (ServiceLinkFor(pageSlug) is not { } link)
            return new(BookingUrl, null, "hero-book", "Записатися", External: true);
        var (key, text) = LinkLabel(link);
        return new("/" + link.Page.Slug, null, key, text, External: false);
    }
```

- [ ] **Step 4: Картка прайсу**

У `ValentynaWeb/Pages/Index.cshtml` замініть:

```cshtml
          @foreach (var servicePage in Model.PublishedPagesFor(category.Id))
          {
            <a href="/@servicePage.Slug" class="price-more" data-i18n-en="@servicePage.CardLinkText.EnAttr">@servicePage.CardLinkText.Ua</a>
          }
```

на:

```cshtml
          @foreach (var link in Model.ServiceLinksFor(category.Id))
          {
            <a href="/@link.Page.Slug" class="price-more" data-i18n-en="@link.Page.CardLinkText.EnAttr">@link.Page.CardLinkText.Ua</a>
          }
```

- [ ] **Step 5: Прибрати зайві методи**

У `ValentynaWeb/Content/Models/ServicePagesContent.cs` видаліть методи `FindPublished` і `PublishedForCategory` разом з їхніми `<summary>` (лишається `FindBySlug`). Перевірка: `grep -rn "FindPublished\|PublishedForCategory\|PublishedPagesFor\|_servicePages" ValentynaWeb --include=*.cs --include=*.cshtml` → порожньо.

- [ ] **Step 6: Перевірити**

Зберіть і запустіть у **Development**:

Run: `node .superpowers/sdd/check-nav.mjs`
Expected:
- `price cards:  /stryzhky /farbuvannia /ukladky /dohliad`
- `stories:      stories-page:/stryzhky stories-page:/ukladky stories-page:/dohliad stories-more:/zavyvka stories-page:/farbuvannia stories-more:/brovy`
- `desktop menu`, `burger menu`, `card pills`, `footer`, усі `others` — `-` (ще не зроблені); `footer quick: # # # # #`.

Зупиніть, запустіть у **Production**:

Run: `node .superpowers/sdd/compare-prod.mjs` → `home: SAME`, `farbuvannia: SAME`.
Run: `node .superpowers/sdd/check-nav.mjs` → `price cards: -`, усі сторіз `hero-book:WLaunch`.

Зупиніть сервер, поверніть змінні середовища.

- [ ] **Step 7: Commit**

```bash
git add ValentynaWeb/Pages/ServicePageLinks.cs ValentynaWeb/Program.cs ValentynaWeb/Pages/Index.cshtml.cs ValentynaWeb/Pages/Index.cshtml ValentynaWeb/Content/Models/ServicePagesContent.cs
git commit -m "feat: ServicePageLinks — на які сторінки послуг посилатися (опубліковані; локально — усі)"
```

---

### Task 2: Меню, плашки на картках, підвал

**Files:**
- Modify: `ValentynaWeb/Pages/Index.cshtml` (меню десктоп і бургер, картки «Послуг», підвал)
- Modify: `ValentynaWeb/Pages/Shared/_Reviews.cshtml` (`id="reviews"`)
- Modify: `ValentynaWeb/wwwroot/css/site.css` (стилі в кінці файлу)

**Interfaces:**
- Consumes: `Model.ServiceLinks`, `Model.ServiceLinkFor(slug)`, `IndexModel.LinkLabel(link)` (задача 1).

- [ ] **Step 1: Пункт «Послуги» в меню на десктопі**

У `ValentynaWeb/Pages/Index.cshtml` у `<ul class="nav-list">` після рядка

```cshtml
            <li><a href="#home" class="nav-link" data-translate="nav-home">Головна</a></li>
```

додайте:

```cshtml
            @* «Послуги» — до карток на головній; при наведенні або фокусі — список сторінок послуг (ServicePageLinks) *@
            <li class="nav-item-services">
              <a href="#services" class="nav-link" data-translate="nav-services">Послуги</a>
              @if (Model.ServiceLinks.Count > 0)
              {
                <div class="nav-dropdown">
                  <ul class="nav-dropdown-list">
                    @foreach (var link in Model.ServiceLinks)
                    {
                      <li><a href="/@link.Page.Slug" class="nav-dropdown-link" data-i18n-en="@link.Page.H1.EnAttr">@link.Page.H1.Ua</a></li>
                    }
                  </ul>
                </div>
              }
            </li>
```

(Скрипт `compare-prod.mjs` вирізає цей `<li class="nav-item-services">…</li>` до першого `</li>` — у Production списку немає, тож вирізається рівно пункт.)

- [ ] **Step 2: Список сторінок у бургер-меню**

Замініть рядок

```cshtml
              <li><a href="#services" class="mobile-nav-link" data-translate="nav-services">Послуги</a></li>
```

на:

```cshtml
              <li>
                <a href="#services" class="mobile-nav-link" data-translate="nav-services">Послуги</a>
                @if (Model.ServiceLinks.Count > 0)
                {
                  <ul class="mobile-nav-sub">
                    @foreach (var link in Model.ServiceLinks)
                    {
                      <li><a href="/@link.Page.Slug" class="mobile-nav-link mobile-nav-sublink" data-i18n-en="@link.Page.H1.EnAttr">@link.Page.H1.Ua</a></li>
                    }
                  </ul>
                }
              </li>
```

(Клас `mobile-nav-link` потрібен, щоб site.js закривав меню після кліку.)

- [ ] **Step 3: Плашка на картці «Послуг»**

У циклі карток замініть:

```cshtml
          <span class="services-text" data-translate="@cat.TitleKey">@cat.Title</span>
        </button>
      </li>
```

на:

```cshtml
          <span class="services-text" data-translate="@cat.TitleKey">@cat.Title</span>
        </button>
        @* картка відкриває сторіз, плашка — сторінку послуги (поверх картки, у лівому верхньому куті) *@
        @if (Model.ServiceLinkFor(cat.Page) is { } link)
        {
          var (labelKey, labelText) = IndexModel.LinkLabel(link);
          <a href="/@link.Page.Slug" class="services-page-link" data-translate="@labelKey">@labelText</a>
        }
      </li>
```

- [ ] **Step 4: Підвал**

Замініть список «Швидкі посилання»:

```cshtml
        <ul class="footer-links">
          <li><a href="#" class="footer-link" data-translate="nav-home">Головна</a></li>
          <li><a href="#" class="footer-link" data-translate="nav-about">Про нас</a></li>
          <li><a href="#" class="footer-link" data-translate="nav-price">Прайс</a></li>
          <li><a href="#" class="footer-link" data-translate="nav-gallery">Галерея</a></li>
          <li><a href="#" class="footer-link" data-translate="reviews-title">Відгуки</a></li>
        </ul>
```

на:

```cshtml
        <ul class="footer-links">
          <li><a href="#home" class="footer-link" data-translate="nav-home">Головна</a></li>
          <li><a href="#about" class="footer-link" data-translate="nav-about">Про нас</a></li>
          <li><a href="#price" class="footer-link" data-translate="nav-price">Прайс</a></li>
          <li><a href="#gallery" class="footer-link" data-translate="nav-gallery">Галерея</a></li>
          <li><a href="#reviews" class="footer-link" data-translate="reviews-title">Відгуки</a></li>
        </ul>
        @* сторінки послуг — у тій самій колонці, щоб не ламати сітку підвалу *@
        @if (Model.ServiceLinks.Count > 0)
        {
          <h3 class="footer-subtitle" data-translate="nav-services">Послуги</h3>
          <ul class="footer-links">
            @foreach (var link in Model.ServiceLinks)
            {
              <li><a href="/@link.Page.Slug" class="footer-link footer-service-link" data-i18n-en="@link.Page.H1.EnAttr">@link.Page.H1.Ua</a></li>
            }
          </ul>
        }
```

У `ValentynaWeb/Pages/Shared/_Reviews.cshtml` замініть `<section class="reviews">` на `<section class="reviews" id="reviews">`.

- [ ] **Step 5: Стилі**

У кінець `ValentynaWeb/wwwroot/css/site.css` додайте:

```css

/* ////////////////// Переходи на сторінки послуг (ServicePageLinks) ////////////////// */

/* Меню на десктопі: «Послуги» з випадним списком сторінок — при наведенні або фокусі.
   padding-top у .nav-dropdown — «місток», щоб список не зникав, поки курсор іде від пункту до нього */
.nav-item-services {
  position: relative;
}

.nav-dropdown {
  position: absolute;
  top: 100%;
  left: 50%;
  z-index: 30;
  display: none;
  padding-top: 12px;
  transform: translateX(-50%);
}

.nav-item-services:hover .nav-dropdown,
.nav-item-services:focus-within .nav-dropdown {
  display: block;
}

.nav-dropdown-list {
  min-width: 240px;
  padding: 8px 0;
  border-radius: 10px;
  background: rgba(31, 27, 25, 0.96);
  box-shadow: 0 16px 36px rgba(0, 0, 0, 0.3);
}

.nav-dropdown-link {
  display: block;
  padding: 9px 20px;
  font-size: 16px;
  white-space: nowrap;
  color: #fff;
}

.nav-dropdown-link:hover,
.nav-dropdown-link:focus-visible {
  background: rgba(255, 255, 255, 0.08);
  color: #ddcdb3;
}

/* Бургер: сторінки послуг під «Послугами»; меню прокручується, якщо пункти не вміщаються на екран */
.mobile-nav-sub {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-top: 16px;
  padding-left: 16px;
}

.mobile-nav-link.mobile-nav-sublink {
  font-size: 16px;
  opacity: 0.85;
}

@media screen and (max-width: 1199px) {
  .mobile-menu {
    overflow-y: auto;
  }
}

/* Картка «Послуг»: плашка на сторінку послуги поверх фото (лічильник фото — праворуч) */
.services-page-link {
  position: absolute;
  top: 12px;
  left: 12px;
  z-index: 2;
  padding: 7px 12px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.92);
  color: #1f1b19;
  font-family: 'Montserrat', sans-serif;
  font-size: 13px;
  font-weight: 600;
  line-height: 1;
}

.services-page-link:hover,
.services-page-link:focus-visible {
  background: #fff;
  color: #8f5f55;
}

/* Підвал: підзаголовок «Послуги» під «Швидкими посиланнями» */
.footer-subtitle {
  margin: 24px 0 10px;
  font-family: 'Montserrat', sans-serif;
  font-weight: 500;
  font-size: 16px;
  color: #3d3c3c;
  text-align: center;
}
```

- [ ] **Step 6: Перевірити**

**Development:** `node .superpowers/sdd/check-nav.mjs` →
- `desktop menu: /stryzhky /ukladky /dohliad /zavyvka /farbuvannia /brovy`
- `burger menu:  /stryzhky /ukladky /dohliad /zavyvka /farbuvannia /brovy`
- `card pills:   /stryzhky /ukladky /dohliad /zavyvka /farbuvannia /brovy`
- `footer:       /stryzhky /ukladky /dohliad /zavyvka /farbuvannia /brovy`
- `footer quick: #home #about #price #gallery #reviews`
- `price cards` і `stories` — як після задачі 1.

**Production:** `node .superpowers/sdd/compare-prod.mjs` → `home: SAME`, `farbuvannia: SAME`; `check-nav.mjs` → `desktop menu`, `burger menu`, `card pills`, `footer` — `-`; `footer quick: #home #about #price #gallery #reviews`.

Зупиніть сервер, поверніть змінні.

- [ ] **Step 7: Commit**

```bash
git add ValentynaWeb/Pages/Index.cshtml ValentynaWeb/Pages/Shared/_Reviews.cshtml ValentynaWeb/wwwroot/css/site.css
git commit -m "feat: переходи на сторінки послуг — меню «Послуги», плашки на картках, підвал; робочі «Швидкі посилання»"
```

---

### Task 3: «Інші послуги» на сторінках послуг

**Files:**
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml.cs`
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml` (блок перед `id="contacts"`)
- Modify: `ValentynaWeb/wwwroot/css/service-page.css` (перед розділом «Фінал і контакти»)

**Interfaces:**
- Consumes: `ServicePageLinks.All()`, `ServicePageLinks.Link` (задача 1).
- Produces: `ServicePageModel.OtherServices` (`IReadOnlyList<ServicePageLinks.Link>`).

- [ ] **Step 1: Модель**

У `ValentynaWeb/Pages/ServicePage.cshtml.cs`:
- додайте поле `private readonly ServicePageLinks _links;` і параметр конструктора `ServicePageLinks links` (присвоєння `_links = links;`), як решту залежностей;
- після властивості `Reviews` (з її коментарем) додайте:

```csharp
    /// <summary>Інші сторінки послуг для блоку «Інші послуги» — ті самі правила, що й посилання з головної (ServicePageLinks)</summary>
    public IReadOnlyList<ServicePageLinks.Link> OtherServices { get; private set; } = [];
```

- в `OnGetAsync` перед `Reviews = await _reviews.GetAsync();` додайте:

```csharp
        OtherServices = _links.All()
            .Where(l => !string.Equals(l.Page.Slug, service.Slug, StringComparison.OrdinalIgnoreCase))
            .ToList();
```

- [ ] **Step 2: Розмітка**

У `ValentynaWeb/Pages/ServicePage.cshtml` перед рядком `<section class="sp-block sp-block-sand" id="contacts">` додайте:

```cshtml
  @if (Model.OtherServices.Count > 0)
  {
    @* Інші послуги: обкладинка картки «Послуг» і назва; телефон — стрічка вбік, десктоп — до п'яти в ряд *@
    <section class="sp-block" id="other-services">
      <div class="sp-wrap">
        <h2 class="sp-title" data-i18n-en="Other services">Інші послуги</h2>
        <ul class="sp-others">
          @foreach (var link in Model.OtherServices)
          {
            <li>
              <a class="sp-other" href="/@link.Page.Slug">
                <responsive-img src="@link.Cover" alt="" sizes="(max-width: 899px) 45vw, 18vw" />
                <span data-i18n-en="@link.Page.H1.EnAttr">@link.Page.H1.Ua</span>
              </a>
            </li>
          }
        </ul>
      </div>
    </section>
  }

```

- [ ] **Step 3: Стилі**

У `ValentynaWeb/wwwroot/css/service-page.css` перед рядком `/* ---------- Фінал і контакти ---------- */` додайте:

```css
/* ---------- Інші послуги ---------- */
/* Телефон: стрічка вбік зі снапом; десктоп: до п'яти в ряд */
.sp-others {
  list-style: none;
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: minmax(140px, 42%);
  gap: 12px;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  padding-bottom: 8px;
}

.sp-others li { scroll-snap-align: start; }

.sp-other {
  display: grid;
  gap: 10px;
  color: inherit;
  font-weight: 600;
  text-decoration: none;
}

.sp-other img {
  width: 100%;
  aspect-ratio: 4 / 5;
  object-fit: cover;
  border-radius: 4px;
}

.sp-other:hover span,
.sp-other:focus-visible span { text-decoration: underline; text-underline-offset: 3px; }

@media (min-width: 900px) {
  .sp-others {
    grid-auto-flow: row;
    grid-template-columns: repeat(5, minmax(0, 1fr));
    overflow: visible;
  }
}

```

- [ ] **Step 4: Перевірити**

**Development:** `node .superpowers/sdd/check-nav.mjs` →
- `others /farbuvannia: /stryzhky /ukladky /dohliad /zavyvka /brovy`
- `others /stryzhky:    /ukladky /dohliad /zavyvka /farbuvannia /brovy`
- `others /ukladky:     /stryzhky /dohliad /zavyvka /farbuvannia /brovy`
- `others /dohliad:     /stryzhky /ukladky /zavyvka /farbuvannia /brovy`
- `others /zavyvka:     /stryzhky /ukladky /dohliad /farbuvannia /brovy`
- `others /brovy:       /stryzhky /ukladky /dohliad /zavyvka /farbuvannia`
- решта рядків — як після задачі 2.

**Production:** `compare-prod.mjs` → `home: SAME`, `farbuvannia: SAME`; усі `others` — `-`.

Зупиніть сервер, поверніть змінні.

- [ ] **Step 5: Commit**

```bash
git add ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Pages/ServicePage.cshtml ValentynaWeb/wwwroot/css/service-page.css
git commit -m "feat: блок «Інші послуги» на сторінках послуг"
```

---

### Task 4: Перевірка в браузері

Виконує координатор (панель браузера, скриншоти) на 5180 у Development:
- [ ] Десктоп 1440: наведення на «Послуги» показує список, він не обрізаний шапкою і клікається; фокус з клавіатури теж відкриває.
- [ ] Телефон 375: бургер — «Послуги» зі списком, меню прокручується до кінця, клік закриває меню і веде на сторінку.
- [ ] Плашки на картках не перекривають лічильник фото і назву; клік по плашці — сторінка, по решті картки — сторіз.
- [ ] «Інші послуги»: телефон — стрічка гортається, десктоп — п'ять у ряд; UA/EN.
- [ ] Підвал: «Швидкі посилання» скролять до розділів; «Послуги» ведуть на сторінки.
- [ ] Консоль без помилок; фінальне рев'ю гілки (від `a6c211f`), звіт Вові зі скриншотами.
