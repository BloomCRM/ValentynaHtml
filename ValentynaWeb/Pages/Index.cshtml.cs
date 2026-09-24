using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IContentProvider _content;

    public IndexModel(IConfiguration config, IContentProvider content)
    {
        _config = config;
        _content = content;
    }

    public string BookingUrl { get; private set; } = "#";
    public IReadOnlyList<ServiceCategory> ServiceCategories { get; private set; } = [];
    public SalonContent Salon { get; private set; } = new();
    public string PhoneDisplay { get; private set; } = "";
    public IReadOnlyList<ScheduleLine> Schedule { get; private set; } = [];

    public void OnGet()
    {
        BookingUrl = _config["Booking:ExternalUrl"]
            ?? "https://w.wlaunch.net/i/bf6cc32e-8bcc-11ef-9c30-2dc04baaba8b/b/bf6dd4b3-8bcc-11ef-9c30-2dc04baaba8b/r";

        ServiceCategories = _content.Services.Categories;
        Salon = _content.Salon;
        PhoneDisplay = FormatPhone(Salon.Phone);
        Schedule = Salon.OpeningHours.Select(ToScheduleLine).ToList();
    }

    public record ScheduleLine(string TranslateKey, string Days, string Time);

    private static readonly Dictionary<string, string> DayAbbrUa = new()
    {
        ["Mon"] = "Пн", ["Tue"] = "Вт", ["Wed"] = "Ср", ["Thu"] = "Чт",
        ["Fri"] = "Пт", ["Sat"] = "Сб", ["Sun"] = "Нд"
    };

    // "+380637250589" → "+38 (063) 725-05-89"; інші формати — як є
    private static string FormatPhone(string phone) =>
        phone.Length == 13 && phone.StartsWith("+380")
            ? $"+38 ({phone[3..6]}) {phone[6..9]}-{phone[9..11]}-{phone[11..13]}"
            : phone;

    // ["Mon".."Sun"] → "Щодня"; ["Mon".."Fri"] → "Пн-Пт". EN-переклад — у site.js за ключем
    private static ScheduleLine ToScheduleLine(OpeningHours h)
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
