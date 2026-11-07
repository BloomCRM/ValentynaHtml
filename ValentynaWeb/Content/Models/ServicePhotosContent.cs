namespace ValentynaWeb.Content.Models;

/// <summary>Картки секції «Послуги» і фото кожної послуги для перегляду-сторіз.</summary>
public class ServicePhotosContent
{
    public List<ServicePhotoCategory> Categories { get; set; } = new();
}

public class ServicePhotoCategory
{
    public string Title { get; set; } = "";
    /// <summary>Ключ EN-перекладу назви у site.js (services-cuts, …)</summary>
    public string? TitleKey { get; set; }

    /// <summary>Id категорії прайсу (services.json), напр. "coloring": якщо в неї є опублікована сторінка, кнопка в сторіс веде туди</summary>
    public string? ServiceId { get; set; }
    /// <summary>Обкладинка картки — шлях без розширення, як у &lt;responsive-img&gt;: "img/cuts"</summary>
    public string Cover { get; set; } = "";
    /// <summary>Фото сторіз — так само без розширення; WebP-варіанти лежать у img/webp/</summary>
    public List<string> Photos { get; set; } = new();
}
