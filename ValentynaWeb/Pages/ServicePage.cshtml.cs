using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValentynaWeb.Content;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Pages;

/// <summary>
/// Сторінка послуги (/farbuvannia …). Тексти — з service-pages.json, невідома адреса → 404.
/// Чернетка (published: false) відкривається за прямим посиланням, але з noindex і без посилань з головної.
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

    public IActionResult OnGet(string slug)
    {
        var service = _content.ServicePages.FindBySlug(slug);
        if (service is null) return NotFound();

        Service = service;
        CanonicalUrl = $"https://{Request.Host}/{service.Slug.ToLowerInvariant()}";
        return Page();
    }
}
