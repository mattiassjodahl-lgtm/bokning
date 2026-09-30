using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BookingDemo.Helpers;

/// <summary>
/// Validerar och föreslår länksegment (slugs) för utbildnings- och nyhetssidor.
///
/// En slug blir en publik adress, så den måste vara unik och tåla att stå i en
/// URL. Dubbletter är det allvarliga felet: uppslagen i WebsiteService tar
/// första träffen, så den andra sidan blir tyst onåbar.
/// </summary>
public static class SlugUtil
{
    /// <summary>Små bokstäver, siffror och enkla bindestreck mellan orden.</summary>
    private static readonly Regex Valid = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private const int MaxLength = 80;

    /// <summary>
    /// Föreslår en slug från fritext: å och ä blir a, ö blir o, allt annat som
    /// inte är a–z eller 0–9 blir bindestreck.
    /// </summary>
    public static string From(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        // Dekomponera och släng diakriterna – å/ä → a, ö → o, é → e.
        var decomposed = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(ch);
        }

        var slug = Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), "[^a-z0-9]+", "-").Trim('-');
        return slug.Length > MaxLength ? slug[..MaxLength].Trim('-') : slug;
    }

    /// <summary>
    /// Returnerar ett felmeddelande, eller null när sluggen duger.
    /// <paramref name="others"/> är sluggarna på övriga poster av samma typ.
    /// </summary>
    public static string? Error(string? slug, IEnumerable<string?> others, bool allowEmpty)
    {
        var value = (slug ?? "").Trim();

        if (value.Length == 0)
            return allowEmpty ? null : "Länk krävs, annars går sidan inte att nå.";

        if (value.Length > MaxLength)
            return $"Länken är för lång, max {MaxLength} tecken.";

        if (!Valid.IsMatch(value))
            return "Endast små bokstäver a–z, siffror och bindestreck. Inga inledande, avslutande eller dubbla bindestreck.";

        if (others.Any(o => string.Equals((o ?? "").Trim(), value, StringComparison.OrdinalIgnoreCase)))
            return "Länken används redan av en annan post.";

        return null;
    }
}
