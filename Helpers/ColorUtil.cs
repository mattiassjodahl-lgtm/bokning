using System.Globalization;

namespace BookingDemo.Helpers;

/// <summary>
/// Små färghjälpare för temasystemet: härleder en mörkare nyans och väljer
/// läsbar textfärg (svart/vit) utifrån bakgrundens ljushet (WCAG-kontrast).
/// </summary>
public static class ColorUtil
{
    /// <summary>Returnerar "#ffffff" eller "#1a1f24" beroende på vad som ger bäst kontrast mot <paramref name="hex"/>.</summary>
    public static string TextOn(string hex)
    {
        var (r, g, b) = Parse(hex);
        // Relativ luminans (förenklad, sRGB-viktad).
        var lum = (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255.0;
        return lum > 0.55 ? "#1a1f24" : "#ffffff";
    }

    /// <summary>Mörkare nyans av en färg (för gradient/hover). factor &lt; 1 = mörkare.</summary>
    public static string Darken(string hex, double factor = 0.80)
    {
        var (r, g, b) = Parse(hex);
        return $"#{Clamp(r * factor):X2}{Clamp(g * factor):X2}{Clamp(b * factor):X2}";
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
