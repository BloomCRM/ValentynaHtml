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
    private static readonly NumberFormatInfo Spaces = new() { NumberGroupSeparator = "\u00A0" };

    /// <summary>From/To = null — ціну не вдалося розібрати: рядок показує текст із прайсу без смужки.
    /// IsFrom — «від N»: є From, але немає To (смужки немає). PriceTranslateKey — переклад текстової ціни.</summary>
    public record Row(string Name, string? TranslateKey, string PriceText, int? From, int? To, string? PriceTranslateKey, bool IsFrom);

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
