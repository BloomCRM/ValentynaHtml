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
