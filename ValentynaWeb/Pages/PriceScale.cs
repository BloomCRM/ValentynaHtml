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
