using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BookingDemo.Models;

namespace BookingDemo.Helpers;

/// <summary>
/// Bygger schema.org-uppmärkning (JSON-LD) för den publika hemsidan.
///
/// Två regler styr innehållet:
///  • Tomma värden utelämnas. Uppmärkningen ska aldrig innehålla tomma eller
///    påhittade fält – den speglar det som faktiskt syns på sidan.
///  • Serialiseringen använder System.Text.Jsons standardencoder, som escapar
///    &lt; &gt; och &amp;. Redigerbart innehåll kan därför inte bryta ut ur
///    script-taggen.
/// </summary>
public static class StructuredData
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(object graph) => JsonSerializer.Serialize(graph, Opts);

    // ── Byggstenar ────────────────────────────────────────────────────────────

    private static Dictionary<string, object?> Node(string type, params (string Key, object? Value)[] props)
        => Node(new object[] { type }, props);

    /// <summary>Skapar en nod och släpper alla tomma värden.</summary>
    private static Dictionary<string, object?> Node(object[] types, params (string Key, object? Value)[] props)
    {
        var d = new Dictionary<string, object?> { ["@type"] = types.Length == 1 ? types[0] : types };

        foreach (var (key, value) in props)
        {
            if (value is null) continue;
            if (value is string s && string.IsNullOrWhiteSpace(s)) continue;
            if (value is System.Collections.ICollection c && c.Count == 0) continue;
            d[key] = value;
        }
        return d;
    }

    private static Dictionary<string, object?> WithContext(Dictionary<string, object?> node)
    {
        var d = new Dictionary<string, object?> { ["@context"] = "https://schema.org" };
        foreach (var kv in node) d[kv.Key] = kv.Value;
        return d;
    }

    private static string? Abs(string baseUrl, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
        return baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    private static bool IsRealUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url) && url != "#" &&
        url.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    /// <summary>Skolan får ett stabilt @id så att övriga noder kan peka på den.</summary>
    private static string SchoolId(string baseUrl) => baseUrl.TrimEnd('/') + "/webb/start#organisation";

    private static Dictionary<string, object?> SchoolRef(string baseUrl) => new() { ["@id"] = SchoolId(baseUrl) };

    // ── Skolan (visas på varje sida via PublicLayout) ─────────────────────────

    public static string School(SchoolProfile school, string baseUrl)
    {
        var node = Node(new object[] { "EducationalOrganization", "LocalBusiness" },
            ("@id", SchoolId(baseUrl)),
            ("name", school.Name),
            ("url", Abs(baseUrl, "/webb/start")),
            ("logo", Abs(baseUrl, school.LogoImage)),
            ("slogan", school.Tagline),
            ("telephone", school.Phone),
            ("email", school.Email),
            ("taxID", school.OrgNumber),
            ("address", Address(school.VisitAddress)),
            ("openingHoursSpecification", OpeningHours(school.OpeningHours)),
            ("sameAs", school.Social.Where(s => IsRealUrl(s.Url)).Select(s => s.Url).ToList()),
            ("memberOf", Node("Organization",
                ("name", "STR – Sveriges Trafikutbildares Riksförbund"),
                ("url", IsRealUrl(school.StrUrl) ? school.StrUrl : null))));

        return Serialize(WithContext(node));
    }

    private static Dictionary<string, object?>? Address(string visitAddress)
    {
        if (string.IsNullOrWhiteSpace(visitAddress)) return null;

        // "Storgatan 12, 652 25 Karlstad" → gata / postnummer / ort.
        var m = Regex.Match(visitAddress, @"^(?<street>.+?),\s*(?<zip>\d{3}\s?\d{2})\s+(?<city>.+)$");

        return m.Success
            ? Node("PostalAddress",
                ("streetAddress", m.Groups["street"].Value.Trim()),
                ("postalCode", m.Groups["zip"].Value.Trim()),
                ("addressLocality", m.Groups["city"].Value.Trim()),
                ("addressCountry", "SE"))
            : Node("PostalAddress",
                ("streetAddress", visitAddress.Trim()),
                ("addressCountry", "SE"));
    }

    // ── Öppettider ────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> DayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["måndag"] = "Monday",  ["tisdag"] = "Tuesday", ["onsdag"] = "Wednesday", ["torsdag"] = "Thursday",
        ["fredag"] = "Friday",  ["lördag"] = "Saturday", ["söndag"] = "Sunday",
    };

    private static readonly string[] WeekOrder =
        { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    private static List<Dictionary<string, object?>> OpeningHours(IEnumerable<OpeningHour> rows)
    {
        var list = new List<Dictionary<string, object?>>();

        foreach (var row in rows)
        {
            var days = ParseDays(row.Days);
            var time = Regex.Match(row.Hours ?? "",
                @"(?<oh>\d{1,2})[.:](?<om>\d{2})\s*[–-]\s*(?<ch>\d{1,2})[.:](?<cm>\d{2})");

            // "Stängt" eller en tid vi inte kan tolka: hoppa över raden hellre än att gissa.
            if (days.Count == 0 || !time.Success) continue;

            list.Add(Node("OpeningHoursSpecification",
                ("dayOfWeek", days.Select(d => "https://schema.org/" + d).ToList()),
                ("opens",  $"{int.Parse(time.Groups["oh"].Value):00}:{time.Groups["om"].Value}"),
                ("closes", $"{int.Parse(time.Groups["ch"].Value):00}:{time.Groups["cm"].Value}")));
        }
        return list;
    }

    /// <summary>Tolkar "Måndag–Torsdag" eller "Fredag" till veckodagar.</summary>
    private static List<string> ParseDays(string? text)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return result;

        foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var range = part.Split('–', '-');

            if (range.Length == 2 &&
                DayNames.TryGetValue(range[0].Trim(), out var from) &&
                DayNames.TryGetValue(range[1].Trim(), out var to))
            {
                var i = Array.IndexOf(WeekOrder, from);
                var j = Array.IndexOf(WeekOrder, to);
                for (var k = i; k != (j + 1) % 7; k = (k + 1) % 7) result.Add(WeekOrder[k]);
            }
            else if (DayNames.TryGetValue(part.Trim(), out var single))
            {
                result.Add(single);
            }
        }
        return result.Distinct().ToList();
    }

    // ── Prislista ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Priserna i affärssystemet är angivna exklusive moms, vilket markeras i
    /// uppmärkningen. Visas priserna inklusive moms på sidan måste båda ändras.
    /// </summary>
    public static string PriceList(IEnumerable<Article> articles, string baseUrl)
    {
        var items = articles.Select((a, i) => Node("ListItem",
            ("position", i + 1),
            ("item", Node("Product",
                ("name", a.Name),
                ("sku", a.ArticleNumber),
                ("url", Abs(baseUrl, "/webb/prislista")),
                ("offers", Node("Offer",
                    ("price", a.Price),
                    ("priceCurrency", "SEK"),
                    ("availability", "https://schema.org/InStock"),
                    ("seller", SchoolRef(baseUrl)),
                    ("priceSpecification", Node("PriceSpecification",
                        ("price", a.Price),
                        ("priceCurrency", "SEK"),
                        ("valueAddedTaxIncluded", false))))))))).ToList();

        return items.Count == 0 ? "" : Serialize(WithContext(Node("ItemList", ("itemListElement", items))));
    }

    // ── Utbildning ────────────────────────────────────────────────────────────

    public static string Course(EducationCard card, string baseUrl)
    {
        var node = Node("Course",
            ("name", card.Title),
            ("description", string.IsNullOrWhiteSpace(card.LongText) ? card.Description : card.LongText),
            ("url", Abs(baseUrl, "/webb/utbildning/" + card.Slug)),
            ("image", Abs(baseUrl, card.Image)),
            ("provider", SchoolRef(baseUrl)));

        return Serialize(WithContext(node));
    }

    // ── Kommande kurser ───────────────────────────────────────────────────────

    /// <summary>Fullbokade kurser märks som SoldOut, så att även en agent ser det.</summary>
    public static string CourseStarts(IEnumerable<CourseItem> courses, string baseUrl)
    {
        var items = courses.OrderBy(c => c.Date).Select((c, i) => Node("ListItem",
            ("position", i + 1),
            ("item", Node("Course",
                ("name", c.Name),
                ("provider", SchoolRef(baseUrl)),
                ("hasCourseInstance", Node("CourseInstance",
                    ("startDate", c.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    ("courseMode", "Onsite"),
                    ("location", SchoolRef(baseUrl)),
                    ("offers", Node("Offer",
                        ("price", c.Price),
                        ("priceCurrency", "SEK"),
                        ("availability", c.Full
                            ? "https://schema.org/SoldOut"
                            : "https://schema.org/InStock"))))))))).ToList();

        return items.Count == 0 ? "" : Serialize(WithContext(Node("ItemList", ("itemListElement", items))));
    }

    // ── Nyhet ─────────────────────────────────────────────────────────────────

    public static string NewsArticle(NewsItem n, string baseUrl)
    {
        var node = Node("Article",
            ("headline", n.Title),
            ("description", n.Summary),
            ("datePublished", n.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("image", Abs(baseUrl, n.Image)),
            ("url", Abs(baseUrl, "/webb/nyheter/" + n.Slug)),
            ("author", SchoolRef(baseUrl)),
            ("publisher", SchoolRef(baseUrl)));

        return Serialize(WithContext(node));
    }

    // ── Brödsmulor ────────────────────────────────────────────────────────────

    public static string Breadcrumbs(string baseUrl, params (string Name, string Path)[] trail)
    {
        var items = trail.Select((t, i) => Node("ListItem",
            ("position", i + 1),
            ("name", t.Name),
            ("item", Abs(baseUrl, t.Path)))).ToList();

        return items.Count == 0 ? "" : Serialize(WithContext(Node("BreadcrumbList", ("itemListElement", items))));
    }
}
