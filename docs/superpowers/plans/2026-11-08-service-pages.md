# Сторінки ще п'яти послуг — план реалізації

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Додати чернетки сторінок `/stryzhky`, `/ukladky`, `/dohliad`, `/zavyvka`, `/brovy` за шаблоном `/farbuvannia`, не змінивши ні «Фарбування», ні головну.

**Architecture:** Кожна сторінка — запис у `Content/service-pages.json`, розмітка — та сама `Pages/ServicePage.cshtml`. Код отримує: вибір частини прайсу з групами (`priceKeys`), головну дію сторінки (`primaryAction`: Telegram або онлайн-запис), зв'язок карток головної зі сторінками за адресою (`page` у `service-photos.json`).

**Tech Stack:** ASP.NET Core 8 Razor Pages, JSON-контент з hot reload (`IContentProvider`), CSS без збирача, vanilla JS (`site.js`, `service-stories.js`). Тестового проєкту немає — перевірка через `curl`/Node-скрипти на запущеному сайті.

**Спека:** `docs/superpowers/specs/2026-11-08-service-pages-design.md` — тексти сторінок звідти дослівно.

## Global Constraints

- Гілка `feat/service-pages`. Не пушити, не відкривати PR, не зливати в `main`.
- У кожному коміті всі сторінки в `service-pages.json` мають `"published": false`.
- **«Фарбування» і головна не змінюються.** HTML `/` і `/farbuvannia` після кожної задачі збігається з еталоном (`.superpowers/sdd/baseline/*.html`) з точністю до хешів `?v=` і пробілів між тегами — скрипт `compare-baseline.mjs` (задача 1).
- Процес на порту 5080 (інша копія сайту) не чіпати. Процеси зупиняти лише за PID, і лише ті, що запустили самі.
- Свій сервер для перевірки — порт **5190** (див. «Як запускати сайт» нижче). Якщо збірка падає через заблоковані файли (`being used by another process`), не вбивати чужі процеси — повідомити BLOCKED.
- Тексти сторінок, підписи кнопок і EN-переклади — дослівно з цього плану (вони збігаються зі спекою).
- Конвенції: C# — PascalCase, namespace `ValentynaWeb…`, коментарі українською, async без `.Result`. CSS-класи сторінок послуг — з префіксом `sp-`. Файли `*.cs`, `*.cshtml`, `*.css` — CRLF (редактор зберігає наявні закінчення рядків), `site.js` — UTF-8 з BOM.
- Повідомлення комітів: `feat:`/`fix:` українською, в кінці — рядок `Co-Authored-By:` моделі, що робила коміт.

## Як запускати сайт для перевірки

З кореня репозиторію (`C:\Users\Vova 2nd\Projects\ValentynaHtml`), у PowerShell:

```powershell
dotnet build ValentynaWeb -nologo -v q
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$app = Start-Process dotnet -ArgumentList 'bin/Debug/net8.0/ValentynaWeb.dll','--urls','http://localhost:5190' -WorkingDirectory 'C:\Users\Vova 2nd\Projects\ValentynaHtml\ValentynaWeb' -PassThru -WindowStyle Hidden
$app.Id
```

Сайт піднімається за 2–5 секунд: `curl -s -o NUL -w "%{http_code}" http://localhost:5190/health` → `200`. Після перевірок: `Stop-Process -Id <Id з $app.Id>`. Перед новою збіркою сервер треба зупинити (інакше dll заблоковані).

Сторінки кешуються на 5 хвилин (output cache): у перевірках додавайте до адреси будь-який `?x=…` — скрипти нижче це роблять самі.

## Файли

| Файл | Що змінюється | Задача |
|---|---|---|
| `ValentynaWeb/Content/Models/ServicePagesContent.cs` | `ServiceCategoryId` необов'язковий, нові `PriceKeys`, `PrimaryAction`; пошук опублікованих сторінок | 1, 4 |
| `ValentynaWeb/Content/service-pages.json` | п'ять нових сторінок-чернеток | 1 |
| `ValentynaWeb/Pages/ServicePage.cshtml.cs` | майстри без категорії, `priceKeys`, головна дія, адреса WLaunch | 1, 2, 3 |
| `ValentynaWeb/Pages/PriceScale.cs` | групи (`Sections`), вибір рядків, крок осі | 2 |
| `ValentynaWeb/Pages/ServicePage.cshtml` | групи на шкалі, кнопки за головною дією, сітка робіт | 2, 3 |
| `ValentynaWeb/wwwroot/css/service-page.css` | підзаголовки груп, сітка на 3 колонки | 2, 3 |
| `ValentynaWeb/Pages/BookingLink.cs` (новий) | адреса WLaunch в одному місці | 3 |
| `ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml` | кнопка в шапці за головною дією | 3 |
| `ValentynaWeb/Pages/Index.cshtml.cs`, `Index.cshtml` | WLaunch з `BookingLink`; посилання в картці прайсу і в сторіз за адресою сторінки | 3, 4 |
| `ValentynaWeb/Content/Models/ServicePhotosContent.cs`, `Content/service-photos.json` | `serviceId` → `page` | 4 |
| `ValentynaWeb/wwwroot/js/site.js` | ключ `stories-more` | 4 |
| `ValentynaWeb/wwwroot/css/site.css` | відступ між двома посиланнями в картці прайсу | 4 |

---

### Task 1: Поля сторінки і п'ять чернеток

**Files:**
- Modify: `ValentynaWeb/Content/Models/ServicePagesContent.cs`
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml.cs`
- Modify: `ValentynaWeb/Content/service-pages.json`
- Create (не в git, тека `.superpowers` ігнорується): `.superpowers/sdd/compare-baseline.mjs`, `.superpowers/sdd/check-pages.mjs`

**Interfaces:**
- Produces: `ServicePage.ServiceCategoryId` (`string?`), `ServicePage.PriceKeys` (`List<string>?`), `ServicePage.PrimaryAction` (`string`, `"telegram"` за замовчуванням або `"booking"`). Скрипти `compare-baseline.mjs` і `check-pages.mjs` використовують задачі 2–5.

- [ ] **Step 1: Скрипт порівняння з еталоном**

Еталон уже знятий до змін: `.superpowers/sdd/baseline/home.html`, `.superpowers/sdd/baseline/farbuvannia.html`. Створіть `.superpowers/sdd/compare-baseline.mjs`:

```js
// Порівнює HTML головної і «Фарбування» з еталоном (.superpowers/sdd/baseline/*.html), знятим до змін.
// Ігнорує хеші ?v=… і пробіли між тегами. Використання: node .superpowers/sdd/compare-baseline.mjs http://localhost:5190
import { readFileSync } from 'node:fs';

const base = process.argv[2] ?? 'http://localhost:5190';
const norm = s => s.replace(/\?v=[\w-]+/g, '').replace(/>\s+</g, '><').replace(/\s+/g, ' ').trim();
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

Примітка: на головній є плашка найближчої події («Сьогодні» / «Завтра» / дата). Якщо єдина відмінність — у цьому тексті (еталон знятий 2026-11-08), це не регресія; напишіть про це у звіті.

- [ ] **Step 2: Скрипт зведення по сторінках**

Створіть `.superpowers/sdd/check-pages.mjs`:

```js
// Зведення по сторінках послуг для перевірки задач плану 2026-11-08-service-pages.
// node .superpowers/sdd/check-pages.mjs [http://localhost:5190]
const base = process.argv[2] ?? 'http://localhost:5190';
const slugs = ['farbuvannia', 'stryzhky', 'ukladky', 'dohliad', 'zavyvka', 'brovy'];

const decode = s => s.replace(/&#x([0-9a-f]+);/gi, (_, h) => String.fromCodePoint(parseInt(h, 16))).replace(/&amp;/g, '&');
const text = s => decode(s).replace(/<[^>]+>/g, ' ').replace(/\s+/g, ' ').trim();
const part = (html, start, end) => {
  const i = html.indexOf(start);
  if (i < 0) return '';
  const j = html.indexOf(end, i + start.length);
  return html.slice(i + start.length, j < 0 ? undefined : j);
};
const links = chunk => [...chunk.matchAll(/<a\b([^>]*)>([\s\S]*?)<\/a>/g)]
  .map(m => `${text(m[2]) || '(icon)'} -> ${((m[1].match(/href="([^"]*)"/) ?? [])[1] ?? '').slice(0, 32)}`);

for (const slug of slugs) {
  const res = await fetch(`${base}/${slug}?check=${Date.now()}`);
  const html = await res.text();
  const storiesJson = part(html, 'id="service-stories-data">', '</script>');
  const stories = storiesJson ? JSON.parse(storiesJson)[0] : null;
  console.log(`\n== /${slug}  ${res.status}  noindex=${html.includes('content="noindex"')}`);
  console.log('h1:        ', text(part(html, '<h1', '</h1>').replace(/^[^>]*>/, '')));
  console.log('hero:      ', links(part(html, 'class="sp-btn-row">', '</div>')).join(' | '));
  console.log('header:    ', links(part(html, '<header class="sp-topbar">', '</header>'))
    .filter(l => !l.includes('-> /') && !l.includes('-> tel:')).join(' | '));
  console.log('bar:       ', links(part(html, 'id="contact-bar">', '</div>')).join(' | '));
  console.log('final:     ', text(part(html, 'id="contacts">', '<address')));
  console.log('works:     ', (html.match(/class="sp-work"/g) ?? []).length, (html.match(/class="(sp-works-grid[^"]*)"/) ?? [])[1] ?? '-');
  console.log('stories:   ', stories ? `${stories.photos.length} photos, cta ${stories.cta.key} -> ${stories.cta.url.slice(0, 32)}` : '-');
  console.log('groups:    ', [...html.matchAll(/class="sp-scale-group"[^>]*>([^<]*)</g)].map(m => decode(m[1])).join(' | ') || '-');
  console.log('price rows:', (html.match(/class="sp-scale-head"/g) ?? []).length);
  console.log('axis:      ', text(part(html, 'class="sp-scale-axis" aria-hidden="true">', '</li>')) || '-');
  console.log('faq:       ', (html.match(/<details>/g) ?? []).length);
}
```

- [ ] **Step 3: Нові поля моделі сторінки**

У `ValentynaWeb/Content/Models/ServicePagesContent.cs` замініть властивість `ServiceCategoryId`:

```csharp
    /// <summary>Id категорії в services.json, звідки беремо ціни: "coloring"</summary>
    public string ServiceCategoryId { get; set; } = "";
```

на:

```csharp
    /// <summary>Id категорії в services.json, звідки беремо ціни: "coloring". null — цін на сторінці немає (завивка, брови)</summary>
    public string? ServiceCategoryId { get; set; }
    /// <summary>Які рядки категорії показати: ключ заголовка групи ("svc-group-wash") — уся група,
    /// ключ рядка ("styling-1") — цей рядок. null або порожньо — уся категорія</summary>
    public List<string>? PriceKeys { get; set; }
    /// <summary>Головна дія: "telegram" — «Надіслати фото в Telegram» (фарбування, завивка);
    /// "booking" — «Записатися» в онлайн-запис WLaunch, Telegram — другою кнопкою</summary>
    public string PrimaryAction { get; set; } = "telegram";
```

- [ ] **Step 4: Майстри для сторінки без категорії**

У `ValentynaWeb/Pages/ServicePage.cshtml.cs` замініть рядок:

```csharp
        Masters = _content.Masters.Masters.Where(m => m.Services.Contains(service.ServiceCategoryId)).ToList();
```

на:

```csharp
        Masters = service.ServiceCategoryId is { } categoryId
            ? _content.Masters.Masters.Where(m => m.Services.Contains(categoryId)).ToList()
            : [];
```

- [ ] **Step 5: П'ять сторінок у `service-pages.json`**

У `ValentynaWeb/Content/service-pages.json` після закривної `}` об'єкта `farbuvannia` (останній елемент масиву `pages`) поставте кому і додайте ці п'ять об'єктів (масив закривається як і раніше — `  ]` і `}`):

```json
    {
      "slug": "stryzhky",
      "published": false,
      "serviceCategoryId": "cuts",
      "primaryAction": "booking",
      "title": "Стрижки в центрі Києва — Перукарня Валентина",
      "description": "Жіночі, чоловічі й дитячі стрижки ножицями та машинкою в центрі Києва, вул. Б. Хмельницького 94. Ціни від 550 грн. Онлайн-запис щодня.",
      "h1": { "ua": "Стрижки", "en": "Haircuts" },
      "lead": {
        "ua": "Жіночі, чоловічі й дитячі стрижки ножицями та машинкою, а ще борода. Запишіться онлайн або напишіть у Telegram, якщо хочете порадитися.",
        "en": "Women's, men's and kids' haircuts with scissors or clippers, plus beard trims. Book online or message us on Telegram if you'd like advice first."
      },
      "heroImage": "img/cuts",
      "heroAlt": "Стрижка в Перукарні Валентина",
      "works": [
        "img/cuts-2",
        "img/cuts-3",
        "img/cuts-4",
        "img/cuts-5",
        "img/cuts-6",
        "img/cuts-7"
      ],
      "priceNote": {
        "ua": "Ціна залежить від довжини й густоти волосся та рангу майстра. Точну суму майстер назве перед стрижкою.",
        "en": "The price depends on hair length and thickness and the stylist's level. The stylist will confirm the exact price before the cut."
      },
      "telegramText": {
        "ua": "Привіт! Пишу з сайту щодо стрижки.",
        "en": "Hi! I'm writing from your website about a haircut."
      },
      "cardLinkText": { "ua": "Детальніше про стрижки", "en": "More about haircuts" },
      "steps": [
        {
          "title": { "ua": "Оберіть час онлайн", "en": "Pick a time online" },
          "text": {
            "ua": "Кнопка «Записатися» відкриває онлайн-запис: послуга і вільний час. Працюємо щодня з 10:00 до 21:00.",
            "en": "The Book button opens online booking with services and free slots. We're open daily from 10:00 to 21:00."
          }
        },
        {
          "title": { "ua": "Покажіть, що хочете", "en": "Show what you want" },
          "text": {
            "ua": "Візьміть фото-референс або опишіть стрижку — майстер підкаже, як вона ляже саме на ваше волосся.",
            "en": "Bring a reference photo or describe the cut — the stylist will tell you how it will work with your hair."
          }
        },
        {
          "title": { "ua": "Стрижка в салоні", "en": "Haircut at the salon" },
          "text": {
            "ua": "Чекаємо вас на Богдана Хмельницького 94. Якщо плани змінились, напишіть у Telegram.",
            "en": "See you at 94 Bohdana Khmelnytskoho St. If your plans change, message us on Telegram."
          }
        }
      ],
      "reviews": [],
      "faq": [
        {
          "question": { "ua": "Скільки коштує стрижка?", "en": "How much is a haircut?" },
          "answer": {
            "ua": "Залежить від довжини волосся й рангу майстра. Усі ціни — на шкалі вище.",
            "en": "It depends on hair length and the stylist's level. All prices are on the scale above."
          }
        },
        {
          "question": { "ua": "Як записатися?", "en": "How do I book?" },
          "answer": {
            "ua": "Онлайн через кнопку «Записатися» — там видно вільний час. Або напишіть у Telegram чи подзвоніть.",
            "en": "Online via the Book button, where you can see free slots. Or message us on Telegram or call."
          }
        },
        {
          "question": { "ua": "Чи стрижете дітей?", "en": "Do you cut kids' hair?" },
          "answer": {
            "ua": "Так, ножицями або машинкою. Ціни — на шкалі вище.",
            "en": "Yes, with scissors or clippers. Prices are on the scale above."
          }
        },
        {
          "question": { "ua": "Скільки триває стрижка?", "en": "How long does a haircut take?" },
          "answer": null
        },
        {
          "question": { "ua": "Що таке стрижка-донат на лікування раку?", "en": "What is the cancer-treatment donation haircut?" },
          "answer": null
        }
      ]
    },
    {
      "slug": "ukladky",
      "published": false,
      "serviceCategoryId": "care",
      "priceKeys": ["styling-1", "styling-2", "styling-3"],
      "primaryAction": "booking",
      "title": "Укладки волосся в центрі Києва — Перукарня Валентина",
      "description": "Укладки волосся трьох рівнів складності в центрі Києва, вул. Б. Хмельницького 94. Ціни від 800 грн. Онлайн-запис щодня з 10:00 до 21:00.",
      "h1": { "ua": "Укладки", "en": "Hair styling" },
      "lead": {
        "ua": "Укладка на щодень чи на особливу подію — три рівні складності. Запишіться онлайн або надішліть референс у Telegram.",
        "en": "Everyday or special-occasion styling, in three levels of complexity. Book online or send a reference on Telegram."
      },
      "heroImage": "img/styling-4",
      "heroAlt": "Укладка в Перукарні Валентина",
      "works": [
        "img/styling-1",
        "img/styling-2",
        "img/styling-3",
        "img/styling-5"
      ],
      "priceNote": {
        "ua": "Що складніша укладка і довше волосся, то вищий рівень. Не знаєте, який рівень потрібен, — надішліть референс у Telegram.",
        "en": "The more complex the styling and the longer the hair, the higher the level. Not sure which level you need? Send a reference on Telegram."
      },
      "telegramText": {
        "ua": "Привіт! Пишу з сайту щодо укладки.",
        "en": "Hi! I'm writing from your website about hair styling."
      },
      "cardLinkText": { "ua": "Детальніше про укладки", "en": "More about styling" },
      "steps": [
        {
          "title": { "ua": "Оберіть час онлайн", "en": "Pick a time online" },
          "text": {
            "ua": "Кнопка «Записатися» відкриває онлайн-запис: послуга і вільний час. Працюємо щодня з 10:00 до 21:00.",
            "en": "The Book button opens online booking with services and free slots. We're open daily from 10:00 to 21:00."
          }
        },
        {
          "title": { "ua": "Надішліть референс", "en": "Send a reference" },
          "text": {
            "ua": "Фото укладки, яку хочете, — у Telegram, щоб майстер підказав рівень і ціну.",
            "en": "Send a photo of the styling you want on Telegram so the stylist can tell you the level and price."
          }
        },
        {
          "title": { "ua": "Укладка в салоні", "en": "Styling at the salon" },
          "text": {
            "ua": "Чекаємо вас на Богдана Хмельницького 94. Якщо плани змінились, напишіть у Telegram.",
            "en": "See you at 94 Bohdana Khmelnytskoho St. If your plans change, message us on Telegram."
          }
        }
      ],
      "reviews": [],
      "faq": [
        {
          "question": { "ua": "Скільки коштує укладка?", "en": "How much is styling?" },
          "answer": {
            "ua": "Залежить від рівня складності — ціни на шкалі вище.",
            "en": "It depends on the level of complexity — prices are on the scale above."
          }
        },
        {
          "question": { "ua": "Як записатися?", "en": "How do I book?" },
          "answer": {
            "ua": "Онлайн через кнопку «Записатися» — там видно вільний час. Або напишіть у Telegram чи подзвоніть.",
            "en": "Online via the Book button, where you can see free slots. Or message us on Telegram or call."
          }
        },
        {
          "question": { "ua": "Чи можна прийти з референсом?", "en": "Can I bring a reference?" },
          "answer": {
            "ua": "Так, візьміть фото — майстер підкаже, що вийде з вашим волоссям.",
            "en": "Yes, bring a photo — the stylist will tell you what will work with your hair."
          }
        },
        {
          "question": { "ua": "Скільки триває укладка?", "en": "How long does styling take?" },
          "answer": null
        }
      ]
    },
    {
      "slug": "dohliad",
      "published": false,
      "serviceCategoryId": "care",
      "priceKeys": ["svc-group-wash", "svc-group-k18", "svc-group-oil", "svc-group-voey"],
      "primaryAction": "booking",
      "title": "Догляд за волоссям у центрі Києва — Перукарня Валентина",
      "description": "Миття голови, догляди K18 і La Biosthetique Oil therapy, пілінг Voey у центрі Києва. Ціни від 800 грн. Онлайн-запис щодня з 10:00 до 21:00.",
      "h1": { "ua": "Догляд за волоссям", "en": "Hair care" },
      "lead": {
        "ua": "Миття голови, відновлювальні догляди K18 і La Biosthetique Oil therapy, пілінг шкіри голови Voey. Запишіться онлайн або напишіть у Telegram, щоб підібрати догляд.",
        "en": "Hair wash, K18 and La Biosthetique Oil therapy repair treatments, Voey scalp peeling. Book online or message us on Telegram to pick a treatment."
      },
      "heroImage": "img/care-3",
      "heroAlt": "Догляд за волоссям у Перукарні Валентина",
      "works": [
        "img/care-1",
        "img/care-2",
        "img/care-3"
      ],
      "priceNote": {
        "ua": "Ціна залежить від довжини волосся. Не знаєте, який догляд обрати, — напишіть у Telegram, майстер підкаже.",
        "en": "The price depends on hair length. Not sure which treatment to choose? Message us on Telegram and a stylist will help."
      },
      "telegramText": {
        "ua": "Привіт! Пишу з сайту щодо догляду за волоссям.",
        "en": "Hi! I'm writing from your website about hair care."
      },
      "cardLinkText": { "ua": "Детальніше про догляд", "en": "More about hair care" },
      "steps": [
        {
          "title": { "ua": "Оберіть час онлайн", "en": "Pick a time online" },
          "text": {
            "ua": "Кнопка «Записатися» відкриває онлайн-запис: послуга і вільний час. Працюємо щодня з 10:00 до 21:00.",
            "en": "The Book button opens online booking with services and free slots. We're open daily from 10:00 to 21:00."
          }
        },
        {
          "title": { "ua": "Майстер оцінить волосся", "en": "The stylist checks your hair" },
          "text": {
            "ua": "І підкаже, який догляд потрібен саме вашому волоссю.",
            "en": "And suggests the treatment your hair needs."
          }
        },
        {
          "title": { "ua": "Догляд у салоні", "en": "Treatment at the salon" },
          "text": {
            "ua": "Чекаємо вас на Богдана Хмельницького 94. Якщо плани змінились, напишіть у Telegram.",
            "en": "See you at 94 Bohdana Khmelnytskoho St. If your plans change, message us on Telegram."
          }
        }
      ],
      "reviews": [],
      "faq": [
        {
          "question": { "ua": "Скільки коштує догляд?", "en": "How much is a treatment?" },
          "answer": {
            "ua": "Залежить від догляду й довжини волосся — ціни на шкалі вище.",
            "en": "It depends on the treatment and hair length — prices are on the scale above."
          }
        },
        {
          "question": { "ua": "Як записатися?", "en": "How do I book?" },
          "answer": {
            "ua": "Онлайн через кнопку «Записатися» — там видно вільний час. Або напишіть у Telegram чи подзвоніть.",
            "en": "Online via the Book button, where you can see free slots. Or message us on Telegram or call."
          }
        },
        {
          "question": { "ua": "Який догляд обрати?", "en": "Which treatment should I choose?" },
          "answer": {
            "ua": "Напишіть у Telegram або запитайте майстра в салоні — він підкаже за станом волосся.",
            "en": "Message us on Telegram or ask the stylist at the salon — they'll advise based on your hair."
          }
        },
        {
          "question": { "ua": "Чим відрізняються K18 і Oil therapy?", "en": "What's the difference between K18 and Oil therapy?" },
          "answer": null
        }
      ]
    },
    {
      "slug": "zavyvka",
      "published": false,
      "primaryAction": "telegram",
      "title": "Хімічна і біозавивка в центрі Києва — Перукарня Валентина",
      "description": "Хімічна та біозавивка волосся в центрі Києва, вул. Б. Хмельницького 94. Надішліть фото в Telegram — майстер підкаже завивку і назве ціну.",
      "h1": { "ua": "Хімічна і біозавивка", "en": "Perm and bio perm" },
      "lead": {
        "ua": "Хімічна та біозавивка: стійкі локони без щоденної укладки. Надішліть фото волосся, і майстер підкаже, яка завивка підійде і скільки вона коштуватиме.",
        "en": "Perm and bio perm: lasting curls without daily styling. Send a photo of your hair and a stylist will suggest the right perm and its price."
      },
      "heroImage": "img/perm-1",
      "heroAlt": "Завивка в Перукарні Валентина",
      "works": [
        "img/perm-2",
        "img/perm-3",
        "img/perm-4",
        "img/perm-5"
      ],
      "telegramText": {
        "ua": "Привіт! Пишу з сайту щодо завивки.",
        "en": "Hi! I'm writing from your website about a perm."
      },
      "steps": [
        {
          "title": { "ua": "Надішліть фото волосся в Telegram", "en": "Send a photo of your hair on Telegram" },
          "text": {
            "ua": "Краще при денному світлі. Напишіть, які локони хочете, — можна з референсом.",
            "en": "Daylight works best. Tell us what curls you'd like — a reference photo helps."
          }
        },
        {
          "title": { "ua": "Майстер підкаже вид завивки й ціну", "en": "The stylist suggests the type of perm and the price" },
          "text": { "ua": "Відповідаємо щодня з 10:00 до 21:00.", "en": "We reply daily from 10:00 to 21:00." }
        },
        {
          "title": { "ua": "Завивка в салоні", "en": "Perm at the salon" },
          "text": {
            "ua": "Оберемо зручний час і чекаємо вас на Богдана Хмельницького 94.",
            "en": "We'll find a convenient time and see you at 94 Bohdana Khmelnytskoho St."
          }
        }
      ],
      "reviews": [],
      "faq": [
        {
          "question": { "ua": "Скільки коштує завивка?", "en": "How much is a perm?" },
          "answer": {
            "ua": "Залежить від довжини й стану волосся. Надішліть фото в Telegram — майстер назве ціну.",
            "en": "It depends on hair length and condition. Send a photo on Telegram and the stylist will give you the price."
          }
        },
        {
          "question": { "ua": "Чим хімічна завивка відрізняється від біозавивки?", "en": "What's the difference between a perm and a bio perm?" },
          "answer": null
        },
        {
          "question": { "ua": "Скільки тримається завивка?", "en": "How long does a perm last?" },
          "answer": null
        }
      ]
    },
    {
      "slug": "brovy",
      "published": false,
      "primaryAction": "booking",
      "title": "Брови в центрі Києва — Перукарня Валентина",
      "description": "Брови в перукарні «Валентина» в центрі Києва, вул. Б. Хмельницького 94. Онлайн-запис щодня з 10:00 до 21:00 або напишіть у Telegram.",
      "h1": { "ua": "Брови", "en": "Brows" },
      "lead": {
        "ua": "Форма й колір брів, що пасують саме вам. Запишіться онлайн або напишіть у Telegram, щоб порадитися з майстром.",
        "en": "Brow shape and colour that suit you. Book online or message us on Telegram to ask the artist."
      },
      "heroImage": "img/brows-1",
      "heroAlt": "Брови в Перукарні Валентина",
      "works": [
        "img/brows-2",
        "img/brows-3",
        "img/brows-4",
        "img/brows-5",
        "img/brows-6",
        "img/brows-7",
        "img/brows-8",
        "img/brows-9"
      ],
      "telegramText": {
        "ua": "Привіт! Пишу з сайту щодо брів.",
        "en": "Hi! I'm writing from your website about brows."
      },
      "steps": [
        {
          "title": { "ua": "Оберіть час онлайн", "en": "Pick a time online" },
          "text": {
            "ua": "Кнопка «Записатися» відкриває онлайн-запис: послуга і вільний час. Працюємо щодня з 10:00 до 21:00.",
            "en": "The Book button opens online booking with services and free slots. We're open daily from 10:00 to 21:00."
          }
        },
        {
          "title": { "ua": "Майстер підбере форму", "en": "The artist picks the shape" },
          "text": { "ua": "Під риси обличчя й ваші побажання.", "en": "To suit your features and wishes." }
        },
        {
          "title": { "ua": "Брови в салоні", "en": "Brows at the salon" },
          "text": {
            "ua": "Чекаємо вас на Богдана Хмельницького 94. Якщо плани змінились, напишіть у Telegram.",
            "en": "See you at 94 Bohdana Khmelnytskoho St. If your plans change, message us on Telegram."
          }
        }
      ],
      "reviews": [],
      "faq": [
        {
          "question": { "ua": "Скільки коштують брови?", "en": "How much do brows cost?" },
          "answer": {
            "ua": "Ціну підкаже адміністратор: напишіть у Telegram або подзвоніть.",
            "en": "The administrator will tell you: message us on Telegram or call."
          }
        },
        {
          "question": { "ua": "Як записатися?", "en": "How do I book?" },
          "answer": {
            "ua": "Онлайн через кнопку «Записатися» — там видно вільний час. Або напишіть у Telegram чи подзвоніть.",
            "en": "Online via the Book button, where you can see free slots. Or message us on Telegram or call."
          }
        },
        {
          "question": { "ua": "Які послуги з бровами робите?", "en": "Which brow services do you offer?" },
          "answer": null
        }
      ]
    }
```

Перевірте, що файл — валідний JSON: `node -e "const p=require('./ValentynaWeb/Content/service-pages.json').pages; console.log(p.map(x=>x.slug+':'+x.published).join(' '))"` → `farbuvannia:false stryzhky:false ukladky:false dohliad:false zavyvka:false brovy:false`.

- [ ] **Step 6: Зібрати, запустити, перевірити**

Запустіть сайт (див. «Як запускати сайт»), потім:

Run: `node .superpowers/sdd/compare-baseline.mjs`
Expected: `home: SAME` і `farbuvannia: SAME`.

Run: `node .superpowers/sdd/check-pages.mjs`
Expected (на цьому кроці кнопки ще всюди Telegram, а Укладки й Догляд показують увесь прайс `care` — це виправлять задачі 2 і 3):
- усі шість сторінок `200  noindex=true`;
- h1: `Фарбування волосся`, `Стрижки`, `Укладки`, `Догляд за волоссям`, `Хімічна і біозавивка`, `Брови`;
- works: farbuvannia 8, stryzhky 6, ukladky 4, dohliad 3, zavyvka 4, brovy 8;
- price rows: farbuvannia 7, stryzhky 13, ukladky 12, dohliad 12, zavyvka 0, brovy 0;
- faq: farbuvannia 1, stryzhky 3, ukladky 3, dohliad 3, zavyvka 1, brovy 2;
- у zavyvka і brovy в hero немає кнопки «Ціни».

Run: `curl -s -o NUL -w "%{http_code}" http://localhost:5190/nema-takoi` → `404`.

Зупиніть сервер (`Stop-Process -Id …`).

- [ ] **Step 7: Commit**

```bash
git add ValentynaWeb/Content/Models/ServicePagesContent.cs ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Content/service-pages.json
git commit -m "feat: чернетки сторінок стрижок, укладок, догляду, завивки і брів (поля priceKeys, primaryAction)"
```

---

### Task 2: Частина прайсу і групи на шкалі цін

**Files:**
- Modify: `ValentynaWeb/Pages/PriceScale.cs`
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml.cs` (рядок `Prices = …`)
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml` (список рядків шкали)
- Modify: `ValentynaWeb/wwwroot/css/service-page.css` (після правила `.sp-scale-axis > span:first-child`)

**Interfaces:**
- Consumes: `ServicePage.PriceKeys` (задача 1).
- Produces: `PriceScale.From(IEnumerable<ServiceItem> services, IReadOnlyCollection<string>? keys = null)`; `PriceScale.Sections` (`IReadOnlyList<PriceScale.Section>`), `record Section(string? Name, string? TranslateKey, IReadOnlyList<Row> Rows)`; `PriceScale.Rows` — усі рядки всіх груп підряд (як і раніше використовується JSON-LD і умовою кнопки «Ціни»). Задача 4 викликає `PriceScale.From(category.Services, page.PriceKeys).Rows.Count`.

- [ ] **Step 1: Групи, вибір рядків і крок осі в `PriceScale`**

У `ValentynaWeb/Pages/PriceScale.cs` замініть усе від `public partial class PriceScale` до кінця методу `From` (тобто константу `TickStep`, `Spaces`, `Row`, властивості, конструктор і `From`) на:

```csharp
public partial class PriceScale
{
    // Крок позначок осі — найменший, при якому позначок не більше трьох:
    // фарбування (до 13 000) — 0 / 5 000 / 10 000, стрижки (до 5 000) — 0 / 2 000 / 4 000, укладки (до 1 500) — 0 / 500 / 1 000
    private static readonly int[] TickSteps = [500, 1000, 2000, 5000, 10000];
    private const int MaxTicks = 3;
    private static readonly NumberFormatInfo Spaces = new() { NumberGroupSeparator = "\u00A0" };

    /// <summary>From/To = null — ціну не вдалося розібрати: рядок показує текст із прайсу без смужки.
    /// IsFrom — «від N»: є From, але немає To (смужки немає). PriceTranslateKey — переклад текстової ціни.</summary>
    public record Row(string Name, string? TranslateKey, string PriceText, int? From, int? To, string? PriceTranslateKey, bool IsFrom);

    /// <summary>Група прайсу з підзаголовком («Стрижки ножицями»). Name = null — рядки до першого заголовка групи, без підзаголовка</summary>
    public record Section(string? Name, string? TranslateKey, IReadOnlyList<Row> Rows);

    public IReadOnlyList<Section> Sections { get; }
    /// <summary>Усі рядки всіх груп підряд — для JSON-LD і кнопки «Ціни»</summary>
    public IReadOnlyList<Row> Rows { get; }
    /// <summary>Кінець шкали; 0 — жодної ціни не розібрано</summary>
    public int Max { get; }
    /// <summary>Позначки осі: 0, крок, 2×крок … (менші за Max)</summary>
    public IReadOnlyList<int> Ticks { get; }

    private PriceScale(List<Section> sections)
    {
        Sections = sections;
        Rows = sections.SelectMany(s => s.Rows).ToList();
        Max = Rows.Max(r => r.To) ?? 0;
        var step = TickSteps.FirstOrDefault(s => (Max + s - 1) / s <= MaxTicks, TickSteps[^1]);
        Ticks = Max == 0
            ? []
            : Enumerable.Range(0, Max / step + 1).Select(i => i * step).Where(v => v < Max).ToList();
    }

    /// <param name="services">Рядки категорії services.json разом із заголовками груп, у порядку прайсу</param>
    /// <param name="keys">Що показати (ServicePage.PriceKeys): ключ заголовка групи — уся група, ключ рядка — цей рядок;
    /// null або порожньо — усе. Ключі на кшталт svc-length-1-2 повторюються в кількох групах — їх вибирають через групу</param>
    public static PriceScale From(IEnumerable<ServiceItem> services, IReadOnlyCollection<string>? keys = null)
    {
        // прайс → групи; рядки до першого заголовка — група без назви
        var groups = new List<(ServiceItem? Header, List<ServiceItem> Items)> { (null, new List<ServiceItem>()) };
        foreach (var item in services)
        {
            if (item.IsGroupHeader) groups.Add((item, new List<ServiceItem>()));
            else groups[^1].Items.Add(item);
        }

        var all = keys is null || keys.Count == 0;
        var sections = new List<Section>();
        foreach (var (header, items) in groups)
        {
            var wholeGroup = all || (header?.TranslateKey is { } groupKey && keys!.Contains(groupKey));
            var picked = wholeGroup
                ? items
                : items.Where(i => i.TranslateKey is { } key && keys!.Contains(key)).ToList();
            if (picked.Count == 0) continue;

            // усередині групи — за ціною «від», як і раніше; групи — в порядку прайсу
            sections.Add(new Section(header?.Name, header?.TranslateKey,
                picked.Select(ToRow).OrderBy(r => r.From ?? int.MaxValue).ToList()));
        }

        return new PriceScale(sections);
    }
```

Решта файлу (`Pct`, `Num`, `ToRow`, `TryParse`, `ParseNumber`, `PriceFormat`) не змінюється. Оновіть `<summary>` класу на:

```csharp
/// <summary>
/// Шкала цін для сторінки послуги: рядки «від–до» по групах прайсу, у групі — за «від»;
/// кінець шкали — найбільша ціна «до», крок позначок осі підбирається під неї.
/// Ціни беремо з services.json (той самий прайс, що й на головній).
/// </summary>
```

- [ ] **Step 2: Сторінка бере свою частину прайсу**

У `ValentynaWeb/Pages/ServicePage.cshtml.cs` замініть:

```csharp
        Prices = PriceScale.From(category?.Services ?? []);
```

на:

```csharp
        Prices = PriceScale.From(category?.Services ?? [], service.PriceKeys);
```

- [ ] **Step 3: Підзаголовки груп на шкалі**

У `ValentynaWeb/Pages/ServicePage.cshtml` замініть блок рядків шкали — від `@foreach (var row in Model.Prices.Rows)` до його закривної `}` (перед `</ul>`):

```cshtml
            @foreach (var row in Model.Prices.Rows)
            {
              <li>
                <div class="sp-scale-head">
                  <span data-translate="@row.TranslateKey">@row.Name</span>
                  @if (row.PriceTranslateKey != null)
                  {
                    <b data-translate="@row.PriceTranslateKey">@row.PriceText</b>
                  }
                  else if (row.IsFrom)
                  {
                    <b><span data-translate="from-price">Від</span> @PriceHtml.Of(row.PriceText)</b>
                  }
                  else
                  {
                    <b>@PriceHtml.Of(row.PriceText)</b>
                  }
                </div>
                @if (row.From is int lo && row.To is int hi)
                {
                  <div class="sp-scale-track" aria-hidden="true"><span class="sp-scale-fill" style="left:@Model.Prices.Pct(lo)%;width:@Model.Prices.Pct(hi - lo)%"></span></div>
                }
              </li>
            }
```

на:

```cshtml
            @foreach (var section in Model.Prices.Sections)
            {
              @* назва групи прайсу («Стрижки ножицями»); EN — за тим самим ключем, що на головній *@
              if (section.Name is not null)
              {
                <li class="sp-scale-group" data-translate="@section.TranslateKey">@section.Name</li>
              }
              foreach (var row in section.Rows)
              {
                <li>
                  <div class="sp-scale-head">
                    <span data-translate="@row.TranslateKey">@row.Name</span>
                    @if (row.PriceTranslateKey != null)
                    {
                      <b data-translate="@row.PriceTranslateKey">@row.PriceText</b>
                    }
                    else if (row.IsFrom)
                    {
                      <b><span data-translate="from-price">Від</span> @PriceHtml.Of(row.PriceText)</b>
                    }
                    else
                    {
                      <b>@PriceHtml.Of(row.PriceText)</b>
                    }
                  </div>
                  @if (row.From is int lo && row.To is int hi)
                  {
                    <div class="sp-scale-track" aria-hidden="true"><span class="sp-scale-fill" style="left:@Model.Prices.Pct(lo)%;width:@Model.Prices.Pct(hi - lo)%"></span></div>
                  }
                </li>
              }
            }
```

(Усередині `@foreach { … }` ми в коді C#, тому `if` і `foreach` — без `@`; Razor-коментар `@* *@` там дозволений.)

- [ ] **Step 4: Стиль підзаголовка групи**

У `ValentynaWeb/wwwroot/css/service-page.css` після рядка `.sp-scale-axis > span:first-child { transform: none; }` додайте:

```css

/* Підзаголовок групи прайсу («Стрижки ножицями», «Догляд К18») */
.sp-scale .sp-scale-group {
  padding: 18px 0 2px;
  font-size: 13px;
  font-weight: 600;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--sp-rose-dark);
}

.sp-scale .sp-scale-axis + .sp-scale-group { padding-top: 6px; }
```

І оновіть коментар над `.sp-scale .sp-scale-axis`: `/* Підписи осі стоять точно на своїх значеннях (0, 5 000, 10 000 з 13 000) */` → `/* Підписи осі стоять точно на своїх значеннях (крок — у PriceScale: 0 / 5 000 / 10 000 з 13 000) */`.

- [ ] **Step 5: Зібрати, запустити, перевірити**

Run: `node .superpowers/sdd/compare-baseline.mjs`
Expected: `home: SAME`, `farbuvannia: SAME`.

Run: `node .superpowers/sdd/check-pages.mjs`
Expected:
- farbuvannia: groups `-`, price rows 7, axis `0 5 000 10 000 грн`;
- stryzhky: groups `Стрижки ножицями | Стрижки з машинкою/чоловічі стрижки`, price rows 13, axis `0 2 000 4 000 грн`;
- ukladky: groups `-`, price rows 3, axis `0 500 1 000 грн`;
- dohliad: groups `Миття голови | Догляд К18 | Догляд La Biosthetique Oil therapy | Пілінг Voey Flake Escape`, price rows 9, axis `0 1 000 2 000 грн`;
- zavyvka, brovy: price rows 0, axis `-`.

Перевірте, що в `ValentynaWeb/wwwroot/js/site.js` є EN-переклади ключів груп: `grep -c "'svc-group-scissors'\|'svc-group-clipper'\|'svc-group-wash'\|'svc-group-k18'\|'svc-group-oil'\|'svc-group-voey'" ValentynaWeb/wwwroot/js/site.js` → `12` (6 ключів × UA і EN). Якщо менше — повідомте у звіті, які відсутні (їх використовує й головна).

Зупиніть сервер.

- [ ] **Step 6: Commit**

```bash
git add ValentynaWeb/Pages/PriceScale.cs ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Pages/ServicePage.cshtml ValentynaWeb/wwwroot/css/service-page.css
git commit -m "feat: шкала цін — частина прайсу (priceKeys), групи з підзаголовками, крок осі під максимум"
```

---

### Task 3: Головна дія сторінки — запис або Telegram

**Files:**
- Create: `ValentynaWeb/Pages/BookingLink.cs`
- Modify: `ValentynaWeb/Pages/Index.cshtml.cs` (рядок `BookingUrl = …`)
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml.cs`
- Modify: `ValentynaWeb/Pages/ServicePage.cshtml` (перший екран, сітка робіт, фінал, нижня панель)
- Modify: `ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml`
- Modify: `ValentynaWeb/wwwroot/css/service-page.css` (сітка робіт)

**Interfaces:**
- Consumes: `ServicePage.PrimaryAction` (задача 1).
- Produces: `BookingLink.From(IConfiguration config)` → `string`; `ServicePageModel.BookingUrl` (`string`), `ServicePageModel.IsBooking` (`bool`).

- [ ] **Step 1: Адреса WLaunch в одному місці**

Створіть `ValentynaWeb/Pages/BookingLink.cs`:

```csharp
namespace ValentynaWeb.Pages;

/// <summary>
/// Онлайн-запис WLaunch: Booking:ExternalUrl з конфігу, інакше — запасна адреса.
/// Одне місце для головної і сторінок послуг, щоб адреси не розійшлися.
/// </summary>
public static class BookingLink
{
    private const string Fallback =
        "https://w.wlaunch.net/i/bf6cc32e-8bcc-11ef-9c30-2dc04baaba8b/b/bf6dd4b3-8bcc-11ef-9c30-2dc04baaba8b/r";

    public static string From(IConfiguration config) => config["Booking:ExternalUrl"] ?? Fallback;
}
```

У `ValentynaWeb/Pages/Index.cshtml.cs` замініть:

```csharp
        BookingUrl = _config["Booking:ExternalUrl"]
            ?? "https://w.wlaunch.net/i/bf6cc32e-8bcc-11ef-9c30-2dc04baaba8b/b/bf6dd4b3-8bcc-11ef-9c30-2dc04baaba8b/r";
```

на:

```csharp
        BookingUrl = BookingLink.From(_config);
```

- [ ] **Step 2: Модель сторінки знає головну дію**

У `ValentynaWeb/Pages/ServicePage.cshtml.cs`:

Поля і конструктор — замініть:

```csharp
    private readonly IContentProvider _content;
    private readonly IReviewsProvider _reviews;

    public ServicePageModel(IContentProvider content, IReviewsProvider reviews)
    {
        _content = content;
        _reviews = reviews;
    }
```

на:

```csharp
    private readonly IContentProvider _content;
    private readonly IReviewsProvider _reviews;
    private readonly IConfiguration _config;

    public ServicePageModel(IContentProvider content, IReviewsProvider reviews, IConfiguration config)
    {
        _content = content;
        _reviews = reviews;
        _config = config;
    }
```

Після властивості `TelegramUrlEn` (з її коментарем) додайте:

```csharp
    /// <summary>Онлайн-запис WLaunch — головна кнопка сторінок з primaryAction "booking"</summary>
    public string BookingUrl { get; private set; } = "";
    /// <summary>Головна дія — «Записатися» (WLaunch), Telegram другою кнопкою; інакше — «Надіслати фото в Telegram»</summary>
    public bool IsBooking => Service.PrimaryAction == "booking";
```

У `OnGetAsync` замініть блок сторіз:

```csharp
        // кнопка внизу сторіз — та сама головна дія сторінки, що й скрізь тут
        Stories = new ServiceStories([new ServiceStories.Category(
            service.H1.Ua, null, service.H1.EnAttr, service.HeroImage, service.Works,
            new ServiceStories.Cta(TelegramUrl, TelegramUrlEn, "stories-tg", "Надіслати фото в Telegram", External: true))]);
```

на:

```csharp
        BookingUrl = BookingLink.From(_config);

        // кнопка внизу сторіз — та сама головна дія сторінки, що й скрізь тут
        var storiesButton = IsBooking
            ? new ServiceStories.Cta(BookingUrl, null, "hero-book", "Записатися", External: true)
            : new ServiceStories.Cta(TelegramUrl, TelegramUrlEn, "stories-tg", "Надіслати фото в Telegram", External: true);
        Stories = new ServiceStories([new ServiceStories.Category(
            service.H1.Ua, null, service.H1.EnAttr, service.HeroImage, service.Works, storiesButton)]);
```

І в `<summary>` класу після речення про ціни додайте: `Головна дія (primaryAction) — Telegram або онлайн-запис WLaunch.`

- [ ] **Step 3: Кнопки на першому екрані**

У `ValentynaWeb/Pages/ServicePage.cshtml` замініть уміст `<div class="sp-btn-row">` першого екрана:

```cshtml
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
```

на:

```cshtml
        <div class="sp-btn-row">
          @if (Model.IsBooking)
          {
            <a class="sp-btn sp-btn-main" href="@Model.BookingUrl" target="_blank" rel="noopener" data-translate="hero-book">Записатися</a>
            <a class="sp-btn sp-btn-line" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
              <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
              <span data-i18n-en="Message us on Telegram">Написати в Telegram</span>
            </a>
          }
          else
          {
          <a class="sp-btn sp-btn-main" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
            <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
            <span data-i18n-en="Send a photo on Telegram">Надіслати фото в Telegram</span>
          </a>
          if (Model.Prices.Rows.Count > 0)
          {
            <a class="sp-btn sp-btn-line" href="#prices" data-i18n-en="Prices">Ціни</a>
          }
          }
        </div>
```

(Гілка `else` лишає розмітку «Фарбування» без змін; усередині блоку коду `if` пишеться без `@`. Якщо хочете вирівняти відступи в `else` — можна: порівняння з еталоном ігнорує пробіли між тегами.)

- [ ] **Step 4: Сітка робіт на 3 колонки для 3 і 6 фото**

У `ValentynaWeb/Pages/ServicePage.cshtml` замініть рядок:

```cshtml
        <div class="sp-works-grid">
```

на:

```cshtml
        @* 3 або 6 фото — 3 колонки, щоб у сітці не було дірок (3 — і на телефоні) *@
        <div class="@(s.Works.Count switch { 3 => "sp-works-grid sp-works-3", 6 => "sp-works-grid sp-works-6", _ => "sp-works-grid" })">
```

У `ValentynaWeb/wwwroot/css/service-page.css` одразу після блоку

```css
@media (min-width: 900px) {
  .sp-works-grid { grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
}
```

додайте:

```css

/* 3 фото — 3 колонки всюди, 6 — 3 колонки на десктопі: без дірок у сітці (класи ставить ServicePage.cshtml) */
.sp-works-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }

@media (min-width: 900px) {
  .sp-works-6 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}
```

- [ ] **Step 5: Фінальний блок**

У `ValentynaWeb/Pages/ServicePage.cshtml` у секції `id="contacts"` замініть:

```cshtml
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
```

на:

```cshtml
        @if (Model.IsBooking)
        {
          <p class="sp-lead" data-i18n-en="Message us on Telegram and a stylist will suggest what suits you.">Напишіть нам у Telegram, і майстер підкаже, що підійде саме вам.</p>
          <div class="sp-btn-row">
            <a class="sp-btn sp-btn-main" href="@Model.BookingUrl" target="_blank" rel="noopener" data-translate="hero-book">Записатися</a>
            <a class="sp-btn sp-btn-line" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
              <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
              <span data-i18n-en="Message us on Telegram">Написати в Telegram</span>
            </a>
          </div>
        }
        else
        {
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
        }
```

- [ ] **Step 6: Нижня панель на телефоні**

У `ValentynaWeb/Pages/ServicePage.cshtml` замініть перше посилання в `<div class="sp-contact-bar" id="contact-bar">`:

```cshtml
  <a class="sp-btn sp-btn-main" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener" aria-label="Надіслати фото в Telegram">
    <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
    <span data-i18n-en="Send a photo">Надіслати фото</span>
  </a>
```

на:

```cshtml
  @if (Model.IsBooking)
  {
    <a class="sp-btn sp-btn-main" href="@Model.BookingUrl" target="_blank" rel="noopener" data-translate="hero-book">Записатися</a>
  }
  else
  {
  <a class="sp-btn sp-btn-main" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener" aria-label="Надіслати фото в Telegram">
    <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
    <span data-i18n-en="Send a photo">Надіслати фото</span>
  </a>
  }
```

І коментар над панеллю `@* Нижня панель на телефоні (до 900px): головна дія завжди під рукою *@` лишається як є.

- [ ] **Step 7: Кнопка в шапці**

У `ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml` замініть:

```cshtml
      <a class="sp-btn sp-btn-main sp-btn-sm sp-topcta" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
        <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
        <span data-i18n-en="Send a photo">Надіслати фото</span>
      </a>
```

на:

```cshtml
      @if (Model.IsBooking)
      {
        <a class="sp-btn sp-btn-main sp-btn-sm sp-topcta" href="@Model.BookingUrl" target="_blank" rel="noopener" data-translate="hero-book">Записатися</a>
      }
      else
      {
      <a class="sp-btn sp-btn-main sp-btn-sm sp-topcta" href="@Model.TelegramUrl" data-i18n-href-en="@Model.TelegramUrlEn" target="_blank" rel="noopener">
        <svg class="sp-icon" aria-hidden="true"><use href="#sp-icon-tg"></use></svg>
        <span data-i18n-en="Send a photo">Надіслати фото</span>
      </a>
      }
```

І в коментарі на початку файлу `Телефон і «Надіслати фото» видно з 900px` → `Телефон і головну дію («Надіслати фото» або «Записатися») видно з 900px`.

- [ ] **Step 8: Зібрати, запустити, перевірити**

Run: `node .superpowers/sdd/compare-baseline.mjs`
Expected: `home: SAME`, `farbuvannia: SAME`.

Run: `node .superpowers/sdd/check-pages.mjs`
Expected:
- stryzhky, ukladky, dohliad, brovy:
  - hero: `Записатися -> https://w.wlaunch.net/i/bf6cc32e | Написати в Telegram -> https://t.me/valentyna_perukarni`;
  - header: `Записатися -> https://w.wlaunch.net/i/bf6cc32e`;
  - bar: `Записатися -> https://w.wlaunch.net/i/bf6cc32e | (icon) -> tel:+380637250589`;
  - final: `Не знаєте, що обрати? Напишіть нам у Telegram, і майстер підкаже, що підійде саме вам. Записатися Написати в Telegram`;
  - stories: `cta hero-book -> https://w.wlaunch.net/i/bf6cc32e`;
- farbuvannia і zavyvka — як раніше: hero починається з `Надіслати фото в Telegram -> https://t.me/…` (у farbuvannia ще `Ціни -> #prices`, у zavyvka — ні), header `Надіслати фото -> https://t.me/…`, stories `cta stories-tg`;
- works grid: stryzhky `sp-works-grid sp-works-6`, dohliad `sp-works-grid sp-works-3`, решта `sp-works-grid`.

Run: `curl -s "http://localhost:5190/stryzhky?tg=1" | grep -o 'href="https://t.me/[^"]*"' | head -1` → посилання з `?text=%D0%9F%D1%80%D0%B8%D0%B2%D1%96%D1%82` (готовий текст українською) і `curl -s "http://localhost:5190/stryzhky?tg=2" | grep -o 'data-i18n-href-en="[^"]*"' | head -1` → `?text=Hi%21%20I%27m%20writing…about%20a%20haircut.`

Зупиніть сервер.

- [ ] **Step 9: Commit**

```bash
git add ValentynaWeb/Pages/BookingLink.cs ValentynaWeb/Pages/Index.cshtml.cs ValentynaWeb/Pages/ServicePage.cshtml.cs ValentynaWeb/Pages/ServicePage.cshtml ValentynaWeb/Pages/Shared/_ServicePageHeader.cshtml ValentynaWeb/wwwroot/css/service-page.css
git commit -m "feat: головна дія сторінки послуги — онлайн-запис або Telegram; сітка робіт без дірок для 3 і 6 фото"
```

---

### Task 4: Посилання з головної — за адресою сторінки

**Files:**
- Modify: `ValentynaWeb/Content/Models/ServicePhotosContent.cs`
- Modify: `ValentynaWeb/Content/service-photos.json`
- Modify: `ValentynaWeb/Content/Models/ServicePagesContent.cs`
- Modify: `ValentynaWeb/Pages/Index.cshtml.cs`
- Modify: `ValentynaWeb/Pages/Index.cshtml` (картка прайсу)
- Modify: `ValentynaWeb/wwwroot/js/site.js` (переклади)
- Modify: `ValentynaWeb/wwwroot/css/site.css` (після `.price-more:hover …`)
- Create (не в git): `.superpowers/sdd/check-home-links.mjs`

**Interfaces:**
- Consumes: `PriceScale.From(services, keys)` (задача 2), `BookingUrl` на головній (задача 3).
- Produces: `ServicePhotoCategory.Page` (`string?`); `ServicePagesContent.FindPublished(string? slug)` → `ServicePage?`; `ServicePagesContent.PublishedForCategory(string? categoryId)` → `IReadOnlyList<ServicePage>`; `IndexModel.PublishedPagesFor(string? categoryId)` → `IReadOnlyList<ServicePage>`. Метод `FindPublishedByCategory` і `IndexModel.PublishedPageFor` видаляються.

- [ ] **Step 1: Скрипт перевірки посилань головної**

Створіть `.superpowers/sdd/check-home-links.mjs`:

```js
// Посилання головної на сторінки послуг: картки прайсу і кнопки в сторіз.
// node .superpowers/sdd/check-home-links.mjs [http://localhost:5190]
const base = process.argv[2] ?? 'http://localhost:5190';
const html = await (await fetch(`${base}/?links=${Date.now()}`)).text();
const decode = s => s.replace(/&#x([0-9a-f]+);/gi, (_, h) => String.fromCodePoint(parseInt(h, 16)));

const cards = [...html.matchAll(/<a href="([^"]+)" class="price-more"[^>]*>([^<]*)</g)].map(m => `${decode(m[2])} -> ${m[1]}`);
console.log('price card links:', cards.length ? cards.join(' | ') : '-');
const data = html.match(/id="service-stories-data">([\s\S]*?)<\/script>/)[1];
for (const c of JSON.parse(data)) console.log(`stories: ${c.title} -> ${c.cta.key} ${c.cta.url.slice(0, 40)}`);
```

Запустіть сайт на поточному коді (до змін цієї задачі) і зафіксуйте вивід — він має лишитися таким самим після змін (сторінки — чернетки):
- `price card links: -`;
- шість рядків `stories: … -> hero-book https://w.wlaunch.net/i/bf6cc32e-8bcc-11`.

Зупиніть сервер.

- [ ] **Step 2: Картка «Послуг» знає адресу своєї сторінки**

У `ValentynaWeb/Content/Models/ServicePhotosContent.cs` замініть:

```csharp
    /// <summary>Id категорії прайсу (services.json), напр. "coloring": якщо в неї є опублікована сторінка, кнопка в сторіс веде туди</summary>
    public string? ServiceId { get; set; }
```

на:

```csharp
    /// <summary>Адреса сторінки послуги з service-pages.json ("farbuvannia"): коли сторінка опублікована, кнопка в сторіс веде туди.
    /// За адресою, а не за категорією прайсу: укладки й догляд ділять одну категорію "care"</summary>
    public string? Page { get; set; }
```

У `ValentynaWeb/Content/service-photos.json`:
- у категорії «Фарбування» замініть рядок `"serviceId": "coloring",` на `"page": "farbuvannia",`;
- у кожну з решти категорій додайте рядок `"page"` одразу після `"titleKey"`:
  - «Стрижки» (`services-cuts`) → `"page": "stryzhky",`
  - «Укладки» (`services-styling`) → `"page": "ukladky",`
  - «Догляд» (`services-care`) → `"page": "dohliad",`
  - «Хім та біо завивка» (`services-chemical`) → `"page": "zavyvka",`
  - «Брови» (`services-brows`) → `"page": "brovy",`

Перевірка: `node -e "console.log(require('./ValentynaWeb/Content/service-photos.json').categories.map(c=>c.title+'='+c.page).join(' | '))"` → `Стрижки=stryzhky | Укладки=ukladky | Догляд=dohliad | Хім та біо завивка=zavyvka | Фарбування=farbuvannia | Брови=brovy`, і `grep -c serviceId ValentynaWeb/Content/service-photos.json` → `0`.

- [ ] **Step 3: Пошук опублікованих сторінок**

У `ValentynaWeb/Content/Models/ServicePagesContent.cs` замініть метод:

```csharp
    /// <summary>Опублікована сторінка категорії прайсу (id з services.json); чернетки не повертаємо.</summary>
    public ServicePage? FindPublishedByCategory(string? categoryId) =>
        string.IsNullOrEmpty(categoryId)
            ? null
            : Pages.FirstOrDefault(p => p.Published && string.Equals(p.ServiceCategoryId, categoryId, StringComparison.Ordinal));
```

на:

```csharp
    /// <summary>Опублікована сторінка за адресою; чернетки не повертаємо — з головної посилаємось лише на опубліковані.</summary>
    public ServicePage? FindPublished(string? slug) => FindBySlug(slug) is { Published: true } page ? page : null;

    /// <summary>Опубліковані сторінки категорії прайсу (id з services.json) — посилання внизу картки прайсу на головній.
    /// Їх може бути кілька: «Миття, укладки та догляд» → укладки і догляд.</summary>
    public IReadOnlyList<ServicePage> PublishedForCategory(string? categoryId) =>
        string.IsNullOrEmpty(categoryId)
            ? []
            : Pages.Where(p => p.Published && string.Equals(p.ServiceCategoryId, categoryId, StringComparison.Ordinal)).ToList();
```

- [ ] **Step 4: Головна — посилання за адресою**

У `ValentynaWeb/Pages/Index.cshtml.cs`:

Замініть:

```csharp
    /// <summary>Опублікована сторінка послуги для категорії (id з services.json) — для посилань з картки прайсу і сторіс; чернетки не показуємо</summary>
    public ServicePage? PublishedPageFor(string? categoryId) => _servicePages.FindPublishedByCategory(categoryId);
```

на:

```csharp
    /// <summary>Опубліковані сторінки послуг категорії прайсу (id з services.json) — посилання внизу картки; чернетки не показуємо</summary>
    public IReadOnlyList<ServicePage> PublishedPagesFor(string? categoryId) => _servicePages.PublishedForCategory(categoryId);
```

У `OnGetAsync` замініть:

```csharp
        BookingUrl = BookingLink.From(_config);
        Stories = new ServiceStories(ServicePhotos.Select(c => new ServiceStories.Category(
            c.Title, c.TitleKey, null, c.Cover, c.Photos, StoryButton(c.ServiceId))).ToList());

        ServiceCategories = _content.Services.Categories;
```

на:

```csharp
        BookingUrl = BookingLink.From(_config);
        // прайс — до сторіз: кнопка в сторіз залежить від того, чи є на сторінці послуги ціни
        ServiceCategories = _content.Services.Categories;
        Stories = new ServiceStories(ServicePhotos.Select(c => new ServiceStories.Category(
            c.Title, c.TitleKey, null, c.Cover, c.Photos, StoryButton(c.Page))).ToList());

```

Замініть метод `StoryButton` (разом з коментарем над ним):

```csharp
    // Кнопка внизу сторіз: послуга зі своєю опублікованою сторінкою (фарбування) веде туди — там ціни
    // й запис через Telegram; решта — в онлайн-запис WLaunch
    private ServiceStories.Cta StoryButton(string? serviceId) =>
        PublishedPageFor(serviceId) is { } page
            ? new("/" + page.Slug, null, "stories-page", "Ціни та запис", External: false)
            : new(BookingUrl, null, "hero-book", "Записатися", External: true);
```

на:

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

- [ ] **Step 5: Картка прайсу — усі сторінки категорії**

У `ValentynaWeb/Pages/Index.cshtml` замініть:

```cshtml
          @if (Model.PublishedPageFor(category.Id) is { } servicePage)
          {
            <a href="/@servicePage.Slug" class="price-more" data-i18n-en="@servicePage.CardLinkText.EnAttr">@servicePage.CardLinkText.Ua</a>
          }
```

на:

```cshtml
          @foreach (var servicePage in Model.PublishedPagesFor(category.Id))
          {
            <a href="/@servicePage.Slug" class="price-more" data-i18n-en="@servicePage.CardLinkText.EnAttr">@servicePage.CardLinkText.Ua</a>
          }
```

У `ValentynaWeb/wwwroot/css/site.css` одразу після блоку

```css
.price-more:hover,
.price-more:focus-visible {
  background: rgba(221, 205, 179, 0.1);
}
```

додайте:

```css

/* дві сторінки в одній картці («Миття, укладки та догляд» → укладки і догляд) — друге посилання ближче до першого */
.price-more + .price-more {
  margin-top: 12px;
}
```

- [ ] **Step 6: Переклад «Детальніше»**

У `ValentynaWeb/wwwroot/js/site.js` (UTF-8 з BOM — редагуйте так, щоб BOM лишився) у словнику `ua` після рядка `    'stories-tg': 'Надіслати фото в Telegram',` додайте:

```js
    'stories-more': 'Детальніше',
```

у словнику `en` після рядка `    'stories-tg': 'Send a photo on Telegram',` додайте:

```js
    'stories-more': 'Learn more',
```

Перевірка BOM: `node -e "console.log(require('fs').readFileSync('ValentynaWeb/wwwroot/js/site.js')[0].toString(16))"` → `ef`.

- [ ] **Step 7: Зібрати, запустити, перевірити (чернетки)**

Run: `node .superpowers/sdd/compare-baseline.mjs`
Expected: `home: SAME`, `farbuvannia: SAME`.

Run: `node .superpowers/sdd/check-home-links.mjs`
Expected: такий самий вивід, як у Step 1 (`price card links: -`, усі сторіз `hero-book` WLaunch).

- [ ] **Step 8: Перевірка з тимчасовою публікацією**

`service-pages.json` до цього кроку закомічений у задачі 1 і в цій задачі не змінюється. Тимчасово опублікуйте всі сторінки (сайт підхопить файл сам за 1–2 секунди):

```bash
node -e "const f='ValentynaWeb/Content/service-pages.json',fs=require('fs');fs.writeFileSync(f,fs.readFileSync(f,'utf8').replaceAll('\"published\": false','\"published\": true'))"
```

Через 2 секунди:

Run: `node .superpowers/sdd/check-home-links.mjs`
Expected:
- `price card links: Детальніше про стрижки -> /stryzhky | Детальніше про фарбування -> /farbuvannia | Детальніше про укладки -> /ukladky | Детальніше про догляд -> /dohliad`;
- stories:
  - `Стрижки -> stories-page /stryzhky`
  - `Укладки -> stories-page /ukladky`
  - `Догляд -> stories-page /dohliad`
  - `Хім та біо завивка -> stories-more /zavyvka`
  - `Фарбування -> stories-page /farbuvannia`
  - `Брови -> stories-more /brovy`

Поверніть чернетки і переконайтеся, що файл не змінений:

```bash
git checkout -- ValentynaWeb/Content/service-pages.json
git status --short ValentynaWeb/Content/service-pages.json
```

Expected: порожній вивід `git status`. Ще раз `node .superpowers/sdd/check-home-links.mjs` → знову `price card links: -`.

Зупиніть сервер.

- [ ] **Step 9: Commit**

```bash
git add ValentynaWeb/Content/Models/ServicePhotosContent.cs ValentynaWeb/Content/service-photos.json ValentynaWeb/Content/Models/ServicePagesContent.cs ValentynaWeb/Pages/Index.cshtml.cs ValentynaWeb/Pages/Index.cshtml ValentynaWeb/wwwroot/js/site.js ValentynaWeb/wwwroot/css/site.css
git commit -m "feat: головна посилається на сторінки послуг за адресою — кілька в картці прайсу, «Детальніше» в сторіз для сторінок без цін"
```

---

### Task 5: Перевірка в браузері

Виконує координатор (потрібна панель браузера і скриншоти). Сайт — на 5180 через `preview_start "valentyna-web"` після збірки останнього коміту.

- [ ] **Step 1:** `compare-baseline.mjs` і `check-pages.mjs` на 5180 — результати як після задачі 4.
- [ ] **Step 2:** Кожна з п'яти нових сторінок, UA і EN, телефон 390 px і десктоп 1440 px (скриншоти скриптом `shoot.js` з чернетки сесії):
  - тексти — зі специфікації; без горизонтальної прокрутки;
  - EN: «Book Now», «Message us on Telegram», назви груп на шкалі, «UAH»;
  - сітка робіт без дірок; фото відкривають сторіз з натиснутого, кнопка внизу сторіз — головна дія.
- [ ] **Step 3:** Клік по «Записатися» → `booking_click`, по Telegram → `telegram_click`, по телефону → `phone_click` у `window.dataLayer` з `link_location` = `hero` / `contacts` / `contact-bar` / `service-stories`.
- [ ] **Step 4:** Консоль без помилок на `/` і всіх шести сторінках.
- [ ] **Step 5:** Фінальне рев'ю всієї гілки (`review-package` від `main`), виправлення, звіт Вові зі скриншотами. Злиття в `main` і деплой — лише з його згоди.
