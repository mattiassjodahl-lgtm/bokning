namespace BookingDemo.Helpers;

/// <summary>Gemensam textbehandling för metabeskrivningar, så att sidan och admins förhandsvisning blir identiska.</summary>
public static class SeoText
{
    public const int MaxDescription = 160;

    /// <summary>Komprimerar blanksteg och kortar vid ordgräns så att sökresultatet inte klipps mitt i ett ord.</summary>
    public static string? Shorten(string? text, int max = MaxDescription)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (clean.Length <= max) return clean;

        var cut = clean[..(max - 1)];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > max / 2) cut = cut[..lastSpace];
        return cut.TrimEnd(' ', ',', '.', ';', ':', '–', '-') + "…";
    }

    /// <summary>Första värdet som inte är tomt.</summary>
    public static string? FirstNonBlank(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
