using System.Text.RegularExpressions;
using BookingDemo.Models;

namespace BookingDemo.Helpers;

/// <summary>De spårningsverktyg som faktiskt ska laddas efter samtycke. Null = inte aktivt.</summary>
public record ActiveTags(string? TagManagerId, string? AnalyticsId, string? MetaPixelId)
{
    public bool Any => TagManagerId is not null || AnalyticsId is not null || MetaPixelId is not null;
}

/// <summary>
/// Format- och kombinationskontroll av spårnings-ID:n som administratören skriver in.
/// ID:n läggs in i skriptadresser och skriptkod, så bara värden som matchar formatet släpps igenom.
/// Samma format kontrolleras i wwwroot/js/tracking.js.
/// </summary>
public static class TrackingIds
{
    private static readonly Regex Ga    = new("^G-[A-Z0-9]{6,12}$",    RegexOptions.Compiled);
    private static readonly Regex Gtm   = new("^GTM-[A-Z0-9]{4,10}$",  RegexOptions.Compiled);
    private static readonly Regex Pixel = new(@"^\d{10,20}$",          RegexOptions.Compiled);

    public static string Normalize(string? id) => (id ?? "").Trim().ToUpperInvariant();

    public static string? AnalyticsError(TrackingSettings t)
    {
        var ga = Normalize(t.AnalyticsId);
        if (ga.Length == 0) return null;
        if (!Ga.IsMatch(ga)) return "Ange ett GA4-ID på formen G-XXXXXXXXXX.";
        return CombinationError(t);
    }

    public static string? TagManagerError(TrackingSettings t)
    {
        var gtm = Normalize(t.TagManagerId);
        if (gtm.Length == 0) return null;
        if (!Gtm.IsMatch(gtm)) return "Ange ett Tag Manager-ID på formen GTM-XXXXXXX.";
        return CombinationError(t);
    }

    public static string? PixelError(TrackingSettings t)
    {
        var pixel = Normalize(t.MetaPixelId);
        return pixel.Length == 0 || Pixel.IsMatch(pixel)
            ? null
            : "Ange pixelns ID. Det är bara siffror, oftast 15 eller 16.";
    }

    /// <summary>Analytics och Tag Manager samtidigt ger dubbelräknade besök, så antingen eller.</summary>
    public static string? CombinationError(TrackingSettings t) =>
        Normalize(t.AnalyticsId).Length > 0 && Normalize(t.TagManagerId).Length > 0
            ? "Ange antingen Analytics-ID eller Tag Manager-ID, inte båda. Lägg Analytics i Tag Manager i stället."
            : null;

    public static bool HasErrors(TrackingSettings t) =>
        AnalyticsError(t) is not null || TagManagerError(t) is not null || PixelError(t) is not null;

    /// <summary>Det som laddas på sidan. Ogiltiga värden ignoreras, även om någon redigerat lagringsfilen för hand.</summary>
    public static ActiveTags Resolve(TrackingSettings t)
    {
        var gtm   = Normalize(t.TagManagerId);
        var ga    = Normalize(t.AnalyticsId);
        var pixel = Normalize(t.MetaPixelId);

        var gtmOk = Gtm.IsMatch(gtm);
        return new ActiveTags(
            gtmOk ? gtm : null,
            !gtmOk && Ga.IsMatch(ga) ? ga : null,
            Pixel.IsMatch(pixel) ? pixel : null);
    }
}
