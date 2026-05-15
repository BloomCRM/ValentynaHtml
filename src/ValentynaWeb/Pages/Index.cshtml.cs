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

    public void OnGet()
    {
        BookingUrl = _config["Booking:ExternalUrl"]
            ?? "https://w.wlaunch.net/i/bf6cc32e-8bcc-11ef-9c30-2dc04baaba8b/b/bf6dd4b3-8bcc-11ef-9c30-2dc04baaba8b/r";

        ServiceCategories = _content.Services.Categories;
    }
}
