namespace ValentynaWeb.Content.Models;

public class NewsContent
{
    public List<NewsItem> Items { get; set; } = new();
}

public class NewsItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Дата для показу людям: "9 березня 2025"</summary>
    public string Date { get; set; } = "";
    /// <summary>Дата події — за нею ділимо на найближчі/минулі</summary>
    public DateOnly EventDate { get; set; }
    /// <summary>Шлях відносно wwwroot, напр. "img/news-desktop1.png"</summary>
    public string? Image { get; set; }
    /// <summary>Ключі EN-перекладу у site.js; null — текст не перекладається</summary>
    public string? TitleKey { get; set; }
    public string? DescriptionKey { get; set; }
    public string? DateKey { get; set; }

    /// <summary>EN-тексти прямо тут, поруч з UA — для нових подій ключі в site.js не потрібні.
    /// В описі абзаци й пункти списку — через \n, на сайті переноси зберігаються.</summary>
    public string? TitleEn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DateEn { get; set; }
}
