using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using ValentynaWeb.Content;

namespace ValentynaWeb.Pages;

[OutputCache(Duration = 3600)]
public class SitemapModel : PageModel
{
    private readonly IContentProvider _content;

    public SitemapModel(IContentProvider content)
    {
        _content = content;
    }

    public IActionResult OnGet()
    {
        var host = Request.Host.Value;
        var lastmod = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // головна + опубліковані сторінки послуг; чернетки (published: false) у карту сайту не потрапляють
        var urls = new List<string> { UrlEntry(host, "", lastmod, "weekly", "1.0") };
        urls.AddRange(_content.ServicePages.Pages
            .Where(p => p.Published)
            .Select(p => UrlEntry(host, p.Slug.ToLowerInvariant(), lastmod, "monthly", "0.8")));

        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            {string.Join("\n", urls)}
            </urlset>
            """;
        return Content(xml, "application/xml");
    }

    private static string UrlEntry(string host, string path, string lastmod, string changefreq, string priority) => $"""
          <url>
            <loc>https://{host}/{path}</loc>
            <lastmod>{lastmod}</lastmod>
            <changefreq>{changefreq}</changefreq>
            <priority>{priority}</priority>
          </url>
        """;
}
