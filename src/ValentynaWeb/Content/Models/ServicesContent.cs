namespace ValentynaWeb.Content.Models;

public class ServicesContent
{
    public List<ServiceCategory> Categories { get; set; } = new();
}

public class ServiceCategory
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string CssClass { get; set; } = "";
    public string RowCssClass { get; set; } = "";
    public List<ServiceItem> Services { get; set; } = new();
}

public class ServiceItem
{
    public string Name { get; set; } = "";
    public string PriceDisplay { get; set; } = "";
    public bool IsGroupHeader { get; set; }
}
