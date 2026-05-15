using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ValentynaWeb.TagHelpers;

/// <summary>
/// Рендерить &lt;picture&gt; з WebP srcset з 4 ширин (480/768/1280/1920px).
/// Файли мають бути у /img/webp/{stem}-{width}.webp.
/// Приклад: &lt;responsive-img src="img/cuts" alt="Стрижки" sizes="(max-width:767px) 100vw, 20vw" /&gt;
/// </summary>
[HtmlTargetElement("responsive-img")]
public class ResponsiveImageTagHelper : TagHelper
{
    /// <summary>Шлях без розширення відносно wwwroot, напр. "img/cuts"</summary>
    public string Src { get; set; } = "";
    public string Alt { get; set; } = "";
    public string Sizes { get; set; } = "100vw";
    /// <summary>CSS-клас для внутрішнього &lt;img&gt;</summary>
    public string? ImgClass { get; set; }
    /// <summary>true → eager + fetchpriority=high (для hero-картинок)</summary>
    public bool Eager { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    private static readonly int[] Widths = [480, 768, 1280, 1920];

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "picture";
        output.TagMode = TagMode.StartTagAndEndTag;

        var stem = Path.GetFileNameWithoutExtension(Src).Replace(" ", "-");
        var srcset = string.Join(", ", Widths.Select(w => $"/img/webp/{stem}-{w}.webp {w}w"));
        var fallback = $"/img/webp/{stem}-1280.webp";

        var loading = Eager ? "eager" : "lazy";
        var fetchPriority = Eager ? " fetchpriority=\"high\"" : "";
        var classAttr = ImgClass != null ? $" class=\"{ImgClass}\"" : "";
        var widthAttr = Width.HasValue ? $" width=\"{Width}\"" : "";
        var heightAttr = Height.HasValue ? $" height=\"{Height}\"" : "";

        output.Content.SetHtmlContent(
            $"""
            <source type="image/webp" srcset="{srcset}" sizes="{Sizes}" />
            <img src="{fallback}" alt="{Alt}" loading="{loading}"{fetchPriority} decoding="async"{classAttr}{widthAttr}{heightAttr} />
            """);
    }
}
