# CLAUDE.md — Контекст для Claude Code

Цей файл читається Claude Code на старті кожної сесії. Тут — ключові факти, рішення і правила. Закомітити в репо щоб не загубити.

---

## Проект

**Назва репо:** `vkostyrko/ValentynaHtml` (буде перейменовано у `valentyna-web` після тижня 5).
**Що це:** публічний сайт перукарні «Валентина» (https://valentynah.com), Київ, вул. Богдана Хмельницького 94.
**Фаза:** Phase 1 — переписування з статичного HTML на ASP.NET Core Razor Pages.
**Тижні:** 1–5.
**Поточний прогрес ТЗ-2:** ~92% (5 комітів зроблено, лишилось 3 пункти).

### Окремий проект — Bloom CRM (не цей репо)
Сайт у Phase 2 (тиждень 9) переключиться з JSON-контенту на **Bloom CRM API**. Bloom — окремий мульти-тенант SaaS, буде у новому репо (`vkostyrko/bloom`). Зараз його **немає** і поки що **не пишемо**.

---

## Технічний стек

- **.NET 8** (LTS до листопада 2026)
- **ASP.NET Core Razor Pages** — серверний HTML, не Blazor для публічного сайту
- **Docker** + multi-stage Dockerfile, non-root user
- **Caddy 2** — reverse proxy, автоматичний HTTPS Let's Encrypt
- **Hetzner Cloud CX22** — €4/міс VPS
- **GitHub Actions** — CI/CD (поки не налаштовано)

### Контент-шар
Phase 1 — **JSON-файли у `Content/`** через `IContentProvider` з hot reload.
Phase 2 — заміна `IContentProvider` на `BloomApiClient` (HttpClient → Bloom API).

### Що НЕ використовуємо
- ❌ Blazor для публічного сайту (поганий SEO)
- ❌ Database у Phase 1 (контент у JSON)
- ❌ React/Vue/Next/Astro (Razor Pages вистачить)
- ❌ Bundler (Vite/Webpack) — поки не треба

---

## Структура репо (фактична)

```
ValentynaHtml/
├── src/
│   └── ValentynaWeb/                              # ASP.NET Core Razor Pages
│       ├── Pages/
│       │   ├── Index.cshtml                        # головна, контент з legacy
│       │   ├── Index.cshtml.cs                     # читає IContentProvider
│       │   ├── Shared/_Layout.cshtml               # SVG sprite + JSON-LD + sticky
│       │   ├── Shared/_JsonLd.cshtml               # Schema.org HairSalon partial
│       │   ├── _ViewImports.cshtml                 # <responsive-img> зареєстровано
│       │   └── _ViewStart.cshtml
│       ├── Content/                                # JSON content layer
│       │   ├── IContentProvider.cs
│       │   ├── JsonContentProvider.cs
│       │   ├── salon.json, services.json, masters.json
│       │   ├── reviews.json, gallery.json, news.json
│       │   └── Models/                             # 6 POCO моделей
│       ├── TagHelpers/
│       │   └── ResponsiveImageTagHelper.cs         # <picture> з srcset
│       ├── wwwroot/
│       │   ├── css/site.css                        # стилі з legacy + sticky/float
│       │   ├── img/                                # сирі картинки
│       │   ├── img/webp/                           # 60 WebP файлів, 4 ширини
│       │   ├── fonts/Chutz-Bold.woff2
│       │   └── favicon.svg
│       ├── Program.cs
│       ├── appsettings.json
│       └── ValentynaWeb.csproj
├── legacy/                                         # старий HTML — референс
│   └── scripts/convert_to_webp.py                  # Python WebP-конвертер
├── Dockerfile                                      # multi-stage, .NET 8
├── docker-compose.yml                              # app + caddy
├── Caddyfile                                       # new.valentynah.com
├── ValentynaWeb.slnx                               # solution (XML формат)
├── .gitignore                                      # повний для .NET
├── CNAME                                           # для GitHub Pages
└── CLAUDE.md                                       # цей файл
```

---

## ⚠️ Важливо — DNS

`valentynah.com` зараз на GitHub Pages. Caddyfile у репо вказує на `new.valentynah.com` (паралельний тестовий домен). DNS-переключення на Hetzner — на тижні 5 ТЗ-4, не зараз.

---

## ✅ Зроблено (5 комітів)

| Коміт | Що |
|---|---|
| `8c4954d` | .NET migration scaffold — legacy/, Razor Pages, Docker, Caddy |
| `410664e` | Day 3-4: migrate assets + HTML to Razor Pages |
| `0bf6ec6` | Quick fixes — gitignore, Caddyfile new.valentynah.com, img paths, page title |
| `4e3a33f` | JSON content layer з hot reload (IContentProvider) |
| `7c8a2e6` | JSON-LD HairSalon, sticky CTA, Telegram float |
| `45faee4` | WebP conversion — ResponsiveImageTagHelper + top-15 img replacements |

## ❌ Залишилось (3 пункти)

1. **`robots.txt` + `sitemap.xml`** як Razor Page routes (`@page "/robots.txt"`, `@page "/sitemap.xml"`). Host брати з `Context.Request.Host` (як у `_JsonLd.cshtml`). Кешувати sitemap через `[OutputCache(Duration = 3600)]`.
2. **`.github/workflows/deploy.yml`** — авто-деплой на push у `main` / `feat/dotnet-migration` через SSH + rsync до Hetzner. Секрети: `HETZNER_HOST`, `HETZNER_USER`, `HETZNER_SSH_KEY`.
3. **DNS-переключення** (ТЗ-4, тиждень 5): Caddyfile повернути на `valentynah.com`, Cloudflare DNS змінити A-запис на IP Hetzner, видалити `CNAME` файл з репо, перейменувати GitHub repo `ValentynaHtml` → `valentyna-web`.

Після завершення цих 3 пунктів — Phase 1 завершена. Phase 2 (Bloom CRM) — окремий новий репо.

---

## Code conventions

- **Файли:** PascalCase для C# (`IndexModel.cs`), kebab-case для CSS (`hero-section`), camelCase у JSON.
- **Namespace:** `ValentynaWeb` (без точки, як вже є).
- **Async/await:** усе I/O — async. Без `.Result`, `.Wait()`.
- **DI lifetime:** Singleton для providers; Scoped для per-request; Transient — рідко.
- **Commit-повідомлення:** `feat:`, `fix:`, `chore:`, `docs:`, `ci:`, `refactor:`. Українська або англ.
- **Гілка:** `feat/dotnet-migration` для всієї міграції. Merge у `main` коли готово до DNS.

### Razor-патерни що вже встановлені
- **Контент** — через `@inject IContentProvider` в `_Layout` і сторінках.
- **Host у meta/JSON-LD** — `Context.Request.Host`, не з конфігу.
- **Картинки** — `<responsive-img src="img/cuts" alt="..." />` (без розширення, TagHelper підставить `.webp` варіанти).
- **CSS-класи зі старого HTML** — лишаємо як є; нових без потреби не плодимо.

---

## Документи планування (за межами репо)

Усе планування і ТЗ — у папці на OneDrive:
`C:\Users\Volodymyr\OneDrive\Documents\Claude\Projects\Перукарня Вадентина\`

- `ТЗ_index.md` — індекс усіх ТЗ зі статусом.
- `Roadmap.html` — візуальний дашборд, відкривається у браузері.
- `ТЗ-2_старт_сайту_valentyna-web.md` — поточне ТЗ (тижні 1–2, ~92% готово).
- `Сайт_valentynah_аналіз_і_покращення.md` — вимоги до сайту, готові сніпети JSON-LD, robots, sitemap, head.
- `План_розвитку_Перукарня_Валентина.md` — стратегічний план.
- `ER_діаграма_CRM.md` — модель Bloom CRM (для Phase 2).
- `Bloom_архітектура_мультитенантності.md` — Tenant/Shop, RLS, API (Phase 2).

---

## Бізнес-контекст (1 параграф)

Сайт — для перукарні в центрі Києва. 15% доля у Володимира за вкладені $15k. Стабільні 0+ під час війни, 6 крісел, реально працюють 2–3. Цей сайт — частина зусиль вийти у плюс. Дизайн уже є (CSS перенесений), задача — переписати на сучасний стек з контролем над контентом. **Не зламати поточний робочий сайт під час DNS-переходу.**

---

## Запитання — задавай

Якщо щось у документах суперечить або не зрозуміло — питай перш ніж робити. Зміни архітектурні без обговорення — не вітаю. План завжди показувати перед діями.
