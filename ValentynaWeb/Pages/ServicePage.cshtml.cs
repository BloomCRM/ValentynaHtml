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
