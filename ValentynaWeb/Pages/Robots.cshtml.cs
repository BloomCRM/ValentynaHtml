using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ValentynaWeb.Pages;

public class RobotsModel : PageModel
{
    public IActionResult OnGet()
    {
        var host = Request.Host.Value;
        var text = $"User-agent: *\nAllow: /\nSitemap: https://{host}/sitemap.xml\n";
        return Content(text, "text/plain");
    }
}
