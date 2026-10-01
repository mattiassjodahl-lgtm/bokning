namespace BookingDemo.Helpers;

/// <summary>Kontroll av webbadresser som administratören skriver in.</summary>
public static class UrlUtil
{
    /// <summary>En komplett http- eller https-adress med domän.</summary>
    public static bool IsValid(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var value = url.Trim();
        return !value.Any(char.IsWhiteSpace)
            && Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && uri.Host.Contains('.');
    }

    /// <summary>Felmeddelande för ett valfritt adressfält. Tomt fält är tillåtet.</summary>
    public static string? Error(string? url) =>
        string.IsNullOrWhiteSpace(url) || IsValid(url)
            ? null
            : "Ange en komplett adress som börjar med https://, t.ex. https://korkortsboken.se.";
}
