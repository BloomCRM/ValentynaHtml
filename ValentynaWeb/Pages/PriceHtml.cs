using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace ValentynaWeb.Pages;

/// <summary>
/// Ціна для розмітки: «1250-2500 грн» → сума + «грн» окремим span з data-translate="currency",
/// щоб в англійській версії site.js підставляв «UAH». Ціна без «грн» у кінці — текстом як є.
/// </summary>
public static class PriceHtml
{
    private const string Uah = "грн";
    private static readonly HtmlString Currency = new($"<span data-translate=\"currency\">{Uah}</span>");

    public static IHtmlContent Of(string price)
    {
        var text = price.TrimEnd();
        if (!text.EndsWith(Uah, StringComparison.Ordinal))
            return new HtmlString(HtmlEncoder.Default.Encode(price));

        var amount = text[..^Uah.Length].TrimEnd();
        return new HtmlContentBuilder().Append(amount).AppendHtml(" ").AppendHtml(Currency);
    }
}
