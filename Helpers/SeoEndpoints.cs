using System.Text;
using System.Xml.Linq;
using BookingDemo.Models;
using BookingDemo.Services;

namespace BookingDemo.Helpers;

/// <summary>
/// /robots.txt och /sitemap.xml för den publika hemsidan. Sitemapen byggs ur samma
/// innehåll som sajten visar: bara synliga sidor, utbildningar med egen sida och nyheter.
/// </summary>
public static class SeoEndpoints
{
    public static void MapSeoEndpoints(this WebApplication app)
    {
        app.MapGet("/robots.txt", (HttpContext ctx) =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow: /admin");
            sb.AppendLine("Disallow: /login");
            sb.AppendLine("Disallow: /planning");
            sb.AppendLine("Disallow: /elevkort");
            sb.AppendLine("Disallow: /_blazor");
            sb.AppendLine();
            sb.AppendLine($"Sitemap: {BaseUrl(ctx)}/sitemap.xml");
            return Results.Text(sb.ToString(), "text/plain", Encoding.UTF8);
        });

        app.MapGet("/sitemap.xml", (HttpContext ctx, WebsiteService web) =>
        {
            var b = BaseUrl(ctx);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var s = web.Settings;
            var urls = new List<XElement> { Url(ns, b + "/webb/start", null) };

            void AddPage(WebPageKey key, string path)
            {
                if (s.IsVisible(key)) urls.Add(Url(ns, b + path, null));
            }

            AddPage(WebPageKey.Ehandel,   "/webb/ehandel");
            AddPage(WebPageKey.Kalender,  "/webb/kalender");
            AddPage(WebPageKey.Prislista, "/webb/prislista");
            AddPage(WebPageKey.Personal,  "/webb/personal");
            AddPage(WebPageKey.Nyheter,   "/webb/nyheter");
            AddPage(WebPageKey.Kontakt,   "/webb/kontakt");

            if (s.Privacy.HasContent) urls.Add(Url(ns, b + "/webb/integritetspolicy", null));

            foreach (var edu in s.EducationCards.Where(c => c.HasPage))
                urls.Add(Url(ns, b + "/webb/utbildning/" + edu.Slug, null));

            foreach (var news in web.NewsByDate.Where(n => !string.IsNullOrEmpty(n.Slug)))
                urls.Add(Url(ns, b + "/webb/nyheter/" + news.Slug, news.Date));

            var root = new XElement(ns + "urlset", urls);
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + root;
            return Results.Text(xml, "application/xml", Encoding.UTF8);
        });
    }

    private static string BaseUrl(HttpContext ctx) => $"{ctx.Request.Scheme}://{ctx.Request.Host}";

    private static XElement Url(XNamespace ns, string loc, DateOnly? lastmod)
    {
        var el = new XElement(ns + "url", new XElement(ns + "loc", loc));
        if (lastmod is { } d) el.Add(new XElement(ns + "lastmod", d.ToString("yyyy-MM-dd")));
        return el;
    }
}
