namespace ValentynaWeb.Pages;

/// <summary>
/// Онлайн-запис WLaunch: Booking:ExternalUrl з конфігу, інакше — запасна адреса.
/// Одне місце для головної і сторінок послуг, щоб адреси не розійшлися.
/// </summary>
public static class BookingLink
{
    private const string Fallback =
        "https://w.wlaunch.net/i/bf6cc32e-8bcc-11ef-9c30-2dc04baaba8b/b/bf6dd4b3-8bcc-11ef-9c30-2dc04baaba8b/r";

    public static string From(IConfiguration config)
    {
        var url = config["Booking:ExternalUrl"];
        return string.IsNullOrWhiteSpace(url) ? Fallback : url;
    }
}
