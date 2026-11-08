using System.Globalization;
using System.Text.RegularExpressions;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Шкала цін для сторінки послуги: рядки «від–до» по групах прайсу, у групі — за «від»;
/// кінець шкали — найбільша ціна «до», крок позначок осі підбирається під неї.
/// Ціни беремо з services.json (той самий прайс, що й на головній).
/// </summary>
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

    /// <summary>Відсоток від кінця шкали для style="left:…%" — з крапкою незалежно від культури сервера</summary>
    public string Pct(int value) =>
        (Max == 0 ? 0 : value * 100.0 / Max).ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>13000 → «13 000» (нерозривний пробіл)</summary>
    public static string Num(int value) => value.ToString("#,0", Spaces);

    // "2000-2500 грн" → 2000…2500; "1 500 грн" → 1500…1500; "від" (PriceFrom) — лише початок, без смужки;
    // усе, що не збігається з форматом цілком, — текст як є, без смужки
    private static Row ToRow(ServiceItem s)
    {
        if (s.PriceTranslateKey == null && TryParse(s.PriceDisplay, out var lo, out var hi))
        {
            if (s.PriceFrom && lo == hi)
                return new Row(s.Name, s.TranslateKey, $"{Num(lo)} грн", lo, null, null, true);
            if (!s.PriceFrom)
            {
                var text = lo == hi ? $"{Num(lo)} грн" : $"{Num(lo)}–{Num(hi)} грн";
                return new Row(s.Name, s.TranslateKey, text, lo, hi, null, false);
            }
        }

        return new Row(s.Name, s.TranslateKey, s.PriceDisplay, null, null, s.PriceTranslateKey, false);
    }

    private static bool TryParse(string display, out int lo, out int hi)
    {
        lo = hi = 0;
        var m = PriceFormat().Match(display ?? "");
        if (!m.Success || !ParseNumber(m.Groups["lo"].Value, out lo))
            return false;
        if (!m.Groups["hi"].Success)
        {
            hi = lo;
            return true;
        }

        return ParseNumber(m.Groups["hi"].Value, out hi) && lo <= hi;
    }

    private static bool ParseNumber(string raw, out int value) =>
        int.TryParse(raw.Replace(" ", "").Replace("\u00A0", ""), NumberStyles.None, CultureInfo.InvariantCulture, out value);

    [GeneratedRegex(@"^\s*(?<lo>\d+(?:[ \u00A0]\d+)*)(?:\s*[-–—]\s*(?<hi>\d+(?:[ \u00A0]\d+)*))?\s*(?:грн\.?)?\s*$")]
    private static partial Regex PriceFormat();
}
