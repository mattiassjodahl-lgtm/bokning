using System.Globalization;

namespace BookingDemo.Helpers;

/// <summary>
/// Färghjälpare för temasystemet: härleder en mörkare nyans och väljer
/// läsbar textfärg (alltid ren svart eller vit) utifrån WCAG-kontrast.
/// </summary>
public static class ColorUtil
{
    private const string Black = "#1a1f24";
    private const string White = "#ffffff";

    /// <summary>Väljer svart eller vit text – den som ger högst kontrast mot bakgrunden.</summary>
    public static string TextOn(string hex)
    {
        var bg = Luminance(hex);
        var contrastWhite = Contrast(bg, 1.0);
        var contrastBlack = Contrast(bg, Luminance(Black));
        return contrastWhite >= contrastBlack ? White : Black;
    }

    /// <summary>Mörkare nyans av en färg (för gradient/hover). factor &lt; 1 = mörkare.</summary>
    public static string Darken(string hex, double factor = 0.80)
    {
        var (r, g, b) = Parse(hex);
        return $"#{Clamp(r * factor):X2}{Clamp(g * factor):X2}{Clamp(b * factor):X2}";
    }

    // ── Interna hjälpare ──────────────────────────────────────────────────────
    private static double Contrast(double l1, double l2)
    {
        var (hi, lo) = l1 >= l2 ? (l1, l2) : (l2, l1);
        return (hi + 0.05) / (lo + 0.05);
    }

    /// <summary>WCAG relativ luminans (0–1) med gammakorrigering.</summary>
    private static double Luminance(string hex)
    {
        var (r, g, b) = Parse(hex);
        return 0.2126 * Lin(r) + 0.7152 * Lin(g) + 0.0722 * Lin(b);
    }

    private static double Lin(int c)
    {
        var s = c / 255.0;
        return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }

    private static int Clamp(double v) => Math.Clamp((int)Math.Round(v), 0, 255);

    private static (int r, int g, int b) Parse(string hex)
    {
        hex = (hex ?? "").Trim().TrimStart('#');
        if (hex.Length == 3) // #abc → #aabbcc
            hex = string.Concat(hex.Select(c => $"{c}{c}"));
        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return (11, 79, 138); // fallback: primärblå
        return (
            int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
            int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
            int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber));
    }
}
