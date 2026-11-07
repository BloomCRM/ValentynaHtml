namespace ValentynaWeb.Pages;

/// <summary>
/// Дані для перегляду фото у форматі сторіз (_ServiceStories + js/service-stories.js).
/// Головна: кожна картка «Послуг» — окрема категорія. Сторінка послуги: одна категорія — її роботи.
/// </summary>
public record ServiceStories(IReadOnlyList<ServiceStories.Category> Categories)
{
    /// <param name="TitleEn">EN-назва прямо з контенту; якщо null — переклад за TitleKey з site.js</param>
    /// <param name="Cover">Аватар у шапці сторіз — шлях без розширення, як у &lt;responsive-img&gt;</param>
    /// <param name="Photos">Фото — так само без розширення; WebP-варіанти лежать у img/webp/</param>
    /// <param name="Cta">Кнопка внизу сторіз</param>
    public record Category(string Title, string? TitleKey, string? TitleEn, string Cover, IReadOnlyList<string> Photos, Cta Cta);

    /// <param name="UrlEn">Адреса для англійської версії (Telegram з англійським текстом); null — та сама</param>
    /// <param name="TranslateKey">Ключ підпису в site.js — за ним перемикач мови міняє текст</param>
    /// <param name="External">Відкривати в новій вкладці (WLaunch, Telegram); своя сторінка — у тій самій</param>
    public record Cta(string Url, string? UrlEn, string TranslateKey, string Text, bool External);
}
