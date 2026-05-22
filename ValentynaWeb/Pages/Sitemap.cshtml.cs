using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace ValentynaWeb.Pages;

[OutputCache(Duration = 3600)]
public class SitemapModel : PageModel
{
    public IActionResult OnGet()
    {
        var host = Request.Host.Value;
        var lastmod = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url>
                <loc>https://{host}/</loc>
                <lastmod>{lastmod}</lastmod>
                <changefreq>weekly</changefreq>
                <priority>1.0</priority>
              </url>
            </urlset>
            """;
        return Content(xml, "application/xml");
    }
}
