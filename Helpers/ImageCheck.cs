using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BookingDemo.Helpers;

/// <summary>Krav på en uppladdad bild. MaxWidth är samma värde som webbläsaren skalar ner till.</summary>
public record ImageRules(int MinWidth = 0, int MinHeight = 0, int MaxWidth = 1600, int MaxKb = 500, bool AllowSvg = false, bool JpgPngOnly = false);

public record ImageCheckResult(string? Error, string Extension = "", string Format = "", int Width = 0, int Height = 0, long Bytes = 0)
{
    public bool Ok => Error is null;

    /// <summary>T.ex. "1600×900 px · 184 kB · WebP". SVG har ingen pixelstorlek.</summary>
    public string Summary => Width > 0
        ? $"{Width}×{Height} px · {ImageCheck.FormatSize(Bytes)} · {Format}"
        : $"{Format} · {ImageCheck.FormatSize(Bytes)}";
}

/// <summary>
/// Serverns kontroll av uppladdade bilder. Webbläsaren skalar och komprimerar redan innan
/// uppladdning, men klienten går att kringgå, så allt kontrolleras igen här: filtyp efter
/// innehållet (inte filnamnet), pixelstorlek och filstorlek. Inga externa beroenden –
/// måtten läses ur filhuvudet.
/// </summary>
public static class ImageCheck
{
    private const int SvgMaxKb = 200;
    private static readonly CultureInfo Sv = new("sv-SE");

    // Skript i en SVG körs när filen öppnas direkt från sajtens egen domän.
    private static readonly Regex SvgUnsafe =
        new(@"<script|<foreignObject|\bon[a-z]+\s*=|javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private enum Kind { Unknown, Png, Jpeg, Webp, Gif, Svg }

    public static ImageCheckResult Inspect(byte[] bytes, ImageRules rules)
    {
        var size = bytes.LongLength;
        var kind = Sniff(bytes);

        if (kind == Kind.Unknown)
            return Fail(rules.AllowSvg
                ? "Filen är ingen bild som stöds. Använd JPG, PNG, WebP, GIF eller SVG."
                : "Filen är ingen bild som stöds. Använd JPG, PNG, WebP eller GIF.");

        if (rules.JpgPngOnly && kind is not (Kind.Png or Kind.Jpeg))
            return Fail("Bilden måste vara JPG eller PNG. WebP, GIF och SVG visas inte i alla tjänster.");

        if (kind == Kind.Svg)
        {
            if (!rules.AllowSvg)
                return Fail("SVG kan inte användas här. Ladda upp JPG, PNG eller WebP.");
            if (SvgUnsafe.IsMatch(Encoding.UTF8.GetString(bytes)))
                return Fail("SVG-filen innehåller skript och kan inte laddas upp.");
            if (size > SvgMaxKb * 1024L)
                return Fail($"SVG-filen är {FormatSize(size)}. Max {SvgMaxKb} kB.");
            return new ImageCheckResult(null, ".svg", "SVG", 0, 0, size);
        }

        if (!TryGetSize(bytes, kind, out var w, out var h))
            return Fail("Bildens storlek gick inte att läsa. Prova en annan fil.");

        if (w < rules.MinWidth || h < rules.MinHeight)
            return Fail($"Bilden är {w}×{h} px. Den måste vara minst {MinText(rules)} för att bli skarp på sidan.");

        if (w > rules.MaxWidth)
            return Fail($"Bilden är {w} px bred. Max {rules.MaxWidth} px.");

        if (rules.MaxKb > 0 && size > rules.MaxKb * 1024L)
            return Fail($"Bilden är {FormatSize(size)}. Max {rules.MaxKb} kB. Prova en mer komprimerad bild.");

        var (ext, format) = kind switch
        {
            Kind.Png  => (".png",  "PNG"),
            Kind.Jpeg => (".jpg",  "JPEG"),
            Kind.Webp => (".webp", "WebP"),
            _         => (".gif",  "GIF"),
        };
        return new ImageCheckResult(null, ext, format, w, h, size);
    }

    public static string FormatSize(long bytes) =>
        bytes < 1024 * 1024
            ? $"{Math.Max(1, (bytes + 512) / 1024)} kB"
            : (bytes / 1048576.0).ToString("0.0", Sv) + " MB";

    private static string MinText(ImageRules r) =>
        r.MinWidth > 0 && r.MinHeight > 0 ? $"{r.MinWidth}×{r.MinHeight} px"
        : r.MinWidth > 0 ? $"{r.MinWidth} px bred"
        : $"{r.MinHeight} px hög";

    private static ImageCheckResult Fail(string error) => new(error);

    // ── Filtyp efter innehåll ─────────────────────────────────────────────────

    private static Kind Sniff(byte[] b)
    {
        if (b.Length >= 24 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return Kind.Png;
        if (b.Length >= 12 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return Kind.Jpeg;
        if (b.Length >= 10 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8') return Kind.Gif;
        if (b.Length >= 30 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                           && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P') return Kind.Webp;

        var head = Encoding.UTF8.GetString(b, 0, Math.Min(b.Length, 2048));
        return head.Contains("<svg", StringComparison.OrdinalIgnoreCase) ? Kind.Svg : Kind.Unknown;
    }

    // ── Pixelstorlek ur filhuvudet ────────────────────────────────────────────

    private static bool TryGetSize(byte[] b, Kind kind, out int w, out int h)
    {
        w = h = 0;
        return kind switch
        {
            Kind.Png  => PngSize(b, out w, out h),
            Kind.Jpeg => JpegSize(b, out w, out h),
            Kind.Gif  => GifSize(b, out w, out h),
            Kind.Webp => WebpSize(b, out w, out h),
            _         => false,
        };
    }

    private static bool PngSize(byte[] b, out int w, out int h)
    {
        w = b[16] << 24 | b[17] << 16 | b[18] << 8 | b[19];
        h = b[20] << 24 | b[21] << 16 | b[22] << 8 | b[23];
        return w > 0 && h > 0;
    }

    private static bool GifSize(byte[] b, out int w, out int h)
    {
        w = b[6] | b[7] << 8;
        h = b[8] | b[9] << 8;
        return w > 0 && h > 0;
    }

    private static bool JpegSize(byte[] b, out int w, out int h)
    {
        w = h = 0;
        var i = 2;
        while (i + 8 < b.Length)
        {
            if (b[i] != 0xFF) { i++; continue; }
            var m = b[i + 1];
            if (m == 0xFF) { i++; continue; }
            if (m == 0x01 || (m >= 0xD0 && m <= 0xD9)) { i += 2; continue; }

            // SOF0–SOF15 bär måtten, utom DHT (C4), JPG (C8) och DAC (CC).
            if (m is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC)
            {
                h = b[i + 5] << 8 | b[i + 6];
                w = b[i + 7] << 8 | b[i + 8];
                return w > 0 && h > 0;
            }
            i += 2 + (b[i + 2] << 8 | b[i + 3]);
        }
        return false;
    }

    private static bool WebpSize(byte[] b, out int w, out int h)
    {
        w = h = 0;
        switch (Encoding.ASCII.GetString(b, 12, 4))
        {
            case "VP8 ": // lossy
                if (b[23] != 0x9D || b[24] != 0x01 || b[25] != 0x2A) return false;
                w = (b[26] | b[27] << 8) & 0x3FFF;
                h = (b[28] | b[29] << 8) & 0x3FFF;
                break;
            case "VP8L": // lossless
                if (b[20] != 0x2F) return false;
                w = 1 + (b[21] | (b[22] & 0x3F) << 8);
                h = 1 + ((b[22] >> 6) | b[23] << 2 | (b[24] & 0x0F) << 10);
                break;
            case "VP8X": // utökat format, t.ex. med transparens
                w = 1 + (b[24] | b[25] << 8 | b[26] << 16);
                h = 1 + (b[27] | b[28] << 8 | b[29] << 16);
                break;
            default:
                return false;
        }
        return w > 0 && h > 0;
    }
}
