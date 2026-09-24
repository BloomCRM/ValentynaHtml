namespace ValentynaWeb.Content.Models;

public class ServicesContent
{
    public List<ServiceCategory> Categories { get; set; } = new();
}

public class ServiceCategory
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Ключ перекладу в site.js (translations[lang][key]).</summary>
    public string? TranslateKey { get; set; }
    public string CssClass { get; set; } = "";
    public string RowCssClass { get; set; } = "";
    public List<ServiceItem> Services { get; set; } = new();
}

public class ServiceItem
{
    public string Name { get; set; } = "";
    public string? TranslateKey { get; set; }
    public string PriceDisplay { get; set; } = "";
    /// <summary>Ключ перекладу для текстової ціни ("БЕЗКОШТОВНО" → "FREE").</summary>
    public string? PriceTranslateKey { get; set; }
    /// <summary>Ціна "від": перед PriceDisplay виводиться перекладне "Від"/"From".</summary>
    public bool PriceFrom { get; set; }
    public bool IsGroupHeader { get; set; }
}
