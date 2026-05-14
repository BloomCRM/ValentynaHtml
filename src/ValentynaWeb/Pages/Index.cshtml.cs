using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ValentynaWeb.Pages;

public class ServiceItem
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public int PriceFrom { get; init; }
}

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;

    public IndexModel(IConfiguration config) => _config = config;

    public string BookingUrl { get; private set; } = "#";

    public IReadOnlyList<ServiceItem> Services { get; private set; } = new List<ServiceItem>();

    public void OnGet()
    {
        BookingUrl = _config["Booking:ExternalUrl"]
            ?? "https://w.wlaunch.net/i/bf6cc32e-8bcc-11ef-9c30-2dc04baaba8b/b/bf6dd4b3-8bcc-11ef-9c30-2dc04baaba8b/r";

        // TODO: замінити на читання з БД / CMS
        Services = new List<ServiceItem>
        {
            new() { Name = "Стрижка",          Description = "Модельна стрижка з укладкою",      PriceFrom = 450  },
            new() { Name = "Фарбування",        Description = "Одноколірне, мелірування, балаяж", PriceFrom = 800  },
            new() { Name = "Укладка",           Description = "Вечірня, весільна, повсякденна",   PriceFrom = 350  },
            new() { Name = "Догляд за волоссям",Description = "Маски, ботокс, ламінування",       PriceFrom = 600  },
            new() { Name = "Брови та вії",      Description = "Корекція, фарбування",             PriceFrom = 300  },
        };
    }
}
