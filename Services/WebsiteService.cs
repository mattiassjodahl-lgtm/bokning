using BookingDemo.Models;

namespace BookingDemo.Services;

// ════════════════════════════════════════════════════════════════════════════
//  WebsiteService – all data för den publika körskole-hemsidan.
//
//  I prototypen mockas allt in-memory. Profil, anställda och webbflaggade
//  produkter skulle i produktion hämtas från affärssystemet. Lediga tider och
//  prislista återanvänder befintliga BookingService (riktig demodata).
// ════════════════════════════════════════════════════════════════════════════
public class WebsiteService
{
    private readonly BookingService _booking;

    public WebsiteService(BookingService booking)
    {
        _booking = booking;
        Settings.WebFlaggedArticleIds = _booking.Articles.Select(a => a.Id).ToList();
    }

    // ── Körskolans profil (mock) ────────────────────────────────────────────
    public SchoolProfile School { get; } = new()
    {
        Name          = "Vägvisarens Trafikskola",
        LogoInitials  = "VT",
        Tagline       = "Din kompletta trafikskola i stan",
        Phone         = "054-21 40 50",
        Email         = "info@vagvisaren.se",
        VisitAddress  = "Storgatan 12, 652 25 Karlstad",
        PostalAddress = "Box 1234, 651 11 Karlstad",
        OrgNumber     = "556714-4356",
        OpeningHours  = new()
        {
            new() { Days = "Måndag–Torsdag", Hours = "08.00–16.30" },
            new() { Days = "Fredag",         Hours = "08.00–16.00" },
            new() { Days = "Lördag–Söndag",  Hours = "Stängt" },
        },
        Social = new()
        {
            new() { Platform = "facebook",  Label = "Facebook",  Url = "#" },
            new() { Platform = "instagram", Label = "Instagram", Url = "#" },
            new() { Platform = "youtube",   Label = "YouTube",   Url = "#" },
        },
    };

    // ── Anställda (mock). ShowOnWeb styrs i affärssystemet. ────────────────────
    public List<Employee> Employees { get; } = new()
    {
        new() { Id = 1, Name = "Anna Lindgren",  Role = "Trafiklärare & VD",   Initials = "AL", ShowOnWeb = true,
                Bio = "Driver skolan sedan 2009 och brinner för trygg, modern körkortsutbildning." },
        new() { Id = 2, Name = "Johan Berg",     Role = "Trafiklärare (B, BE)", Initials = "JB", ShowOnWeb = true,
                Bio = "Specialist på släp och landsvägskörning. Lugn och pedagogisk." },
        new() { Id = 3, Name = "Sara Ek",        Role = "Trafiklärare (A, AM)", Initials = "SE", ShowOnWeb = true,
                Bio = "MC- och mopedutbildning. Trygg start för nya förare." },
        new() { Id = 4, Name = "Mehmet Demir",   Role = "Administration",       Initials = "MD", ShowOnWeb = false,
                Bio = "Sköter bokningar och fakturor – syns ej på webben." },
    };

    /// <summary>Endast anställda som är webbsynliga.</summary>
    public IEnumerable<Employee> WebEmployees => Employees.Where(e => e.ShowOnWeb);

    // ── Hemsidans inställningar + startsidans innehåll (mock) ──────────────────
    public WebsiteSettings Settings { get; } = new()
    {
        Layout = StartLayout.FocusNews,
        PageVisibility = new()
        {
            { WebPageKey.Ehandel,  true },
            { WebPageKey.Kalender, true },
            { WebPageKey.Prislista, true },
            { WebPageKey.Personal, true },
            { WebPageKey.Kontakt,  true },
        },
        HeroHeading    = "Ta körkort hos Vägvisarens Trafikskola",
        HeroIngress    = "Vi utbildar för bil, släp, moped och MC – med erfarna lärare och en trygg, modern utbildning hela vägen till uppkörningen.",
        HeroImage      = "/Img/Webb/michael-kahn-Hfyrh5vUhuY-unsplash.jpg",
        PrimaryCtaText = "Boka utbildning",
        PrimaryCtaHref = "/webb/kalender",
        SecondaryCtaText = "Kontakta oss",
        SecondaryCtaHref = "/webb/kontakt",
        EducationCards = new()
        {
            new()
            {
                Title = "Personbil (B)", Description = "Manuell och automat. Hela vägen till uppkörning.",
                Icon = "directions_car", Image = "/Img/Webb/art-markiv-zAm1sdicGXc-unsplash.jpg",
                Slug = "personbil-b", PriceFrom = "Från 950 kr/lektion",
                LongText = "B-körkortet ger dig friheten att köra personbil. Hos oss får du en utbildning anpassad efter din nivå – oavsett om du är nybörjare eller har kört förut. Du väljer manuell eller automat, och vi planerar upplägget tillsammans så att du är trygg hela vägen till uppkörningen.",
                Highlights = new() { "Introduktionslektion och nivåbedömning", "Manuell eller automatväxlad bil", "Teori varvat med körning", "Förberedelse inför kunskapsprov och uppkörning" },
            },
            new()
            {
                Title = "Släp (BE/B96)", Description = "Kör lagligt med tyngre släp och husvagn.",
                Icon = "rv_hookup", Image = "/Img/Webb/rolando-garrido-R4y4_dvpYXU-unsplash.jpg",
                Slug = "slap-be-b96", PriceFrom = "Från 1 200 kr/lektion",
                LongText = "Med B96 eller BE får du dra tyngre släp, husvagn eller hästtransport. Vi går igenom koppling, lastsäkring och backning med släp så att du känner dig säker även med last bakom bilen.",
                Highlights = new() { "Koppling och lastsäkring", "Backning och manövrering med släp", "Anpassat för B96 eller fullt BE", "Förberedelse inför uppkörning" },
            },
            new()
            {
                Title = "Moped & EU-moped", Description = "AM-kurs för dig som vill ut på vägarna.",
                Icon = "two_wheeler", Image = "/Img/Webb/martin-nano-yOcUZn6jILI-unsplash.jpg",
                Slug = "moped-am", PriceFrom = "9 995 kr (helkurs)",
                LongText = "AM-körkortet låter dig köra EU-moped från 15 års ålder. Vår kurs ger dig både teorin och den praktiska körningen du behöver för att klara provet och köra säkert i trafiken.",
                Highlights = new() { "Teori om trafikregler och säkerhet", "Praktisk körning i trafikmiljö", "Kursmaterial ingår", "Små grupper" },
            },
            new()
            {
                Title = "Riskutbildning", Description = "Risk 1 och Risk 2 – obligatoriskt inför körkort.",
                Icon = "warning", Image = "/Img/Webb/bas-peperzak-tyhpK_QelPo-unsplash.jpg",
                Slug = "riskutbildning", PriceFrom = "Från 1 200 kr",
                LongText = "Riskutbildningen är obligatorisk för B-körkort. Risk 1 handlar om alkohol, droger, trötthet och riskfaktorer. Risk 2 är den praktiska halkbanan där du tränar på att hantera svåra situationer.",
                Highlights = new() { "Risk 1 – teori om riskfaktorer", "Risk 2 – praktisk halkbana", "Krävs innan uppkörning", "Boka flera tillfällen via kalendern" },
            },
        },
        News = new()
        {
            new() { Title = "Sommarens intensivkurser är öppna",
                    Body  = "Ta körkortet snabbt i sommar – boka en intensivkurs med start i juli. Begränsat antal platser.",
                    Date  = new DateOnly(2026, 6, 15) },
            new() { Title = "Nya automatbilar i flottan",
                    Body  = "Vi har utökat med två nya automatväxlade bilar för en bekvämare körupplevelse.",
                    Date  = new DateOnly(2026, 5, 28) },
            new() { Title = "Risk 2 (halkbana) – extra tillfällen",
                    Body  = "Vi lägger till fler tider för halkbana under hösten. Boka via kalendern.",
                    Date  = new DateOnly(2026, 5, 10) },
        },
    };

    /// <summary>Hämtar en utbildning/behörighet via dess slug. Null om den inte finns.</summary>
    public EducationCard? GetEducation(string slug) =>
        Settings.EducationCards.FirstOrDefault(c => c.Slug == slug);

    // ── Prislista: webbflaggade produkter från affärssystemet ─────────────────
    public IEnumerable<Article> WebProducts =>
        _booking.Articles.Where(a => Settings.WebFlaggedArticleIds.Contains(a.Id));

    /// <summary>Grupperar en artikel till en prislistekategori (baserat på artikelnummer).</summary>
    public static string ProductCategoryFor(Article a)
    {
        if (a.ArticleNumber.StartsWith("RISK", StringComparison.OrdinalIgnoreCase)) return "Riskutbildning";
        if (a.ArticleNumber.StartsWith("UPP", StringComparison.OrdinalIgnoreCase) ||
            a.ArticleNumber.StartsWith("TEO", StringComparison.OrdinalIgnoreCase))  return "Prov & avgifter";
        return "Körlektioner";
    }

    /// <summary>Prislistekategorier i fast ordning (för filterknappar).</summary>
    public static readonly string[] ProductCategories =
        { "Körlektioner", "Riskutbildning", "Prov & avgifter" };

    // ── Lediga tider: speglar körskolans kalender (riktig demodata) ───────────
    /// <summary>Lediga (obokade) tider från idag och <paramref name="days"/> dagar framåt, grupperade per dag.</summary>
    public IReadOnlyList<AvailableDay> GetAvailableSlots(int days = 14)
    {
        var from = DateTime.Today;
        var to   = from.AddDays(days);

        // Visa alla obokade, bokningsbara pass (inga lärar-/resursfilter på publika sidan).
        var filter = new CalendarFilter { ShowAvailable = true, ShowBooked = false };

        return _booking.GetEventsForRange(from, to, filter)
            .Where(e => !e.IsBooked)
            .GroupBy(e => DateOnly.FromDateTime(e.StartTime))
            .OrderBy(g => g.Key)
            .Select(g => new AvailableDay(
                g.Key,
                g.OrderBy(e => e.StartTime)
                 .Select(e => new AvailableSlot(
                     e.StartTime,
                     e.EndTime,
                     _booking.GetLessonType(e.LessonTypeId ?? 0)?.Name ?? "Körlektion",
                     BehorighetFor(e.LessonTypeId ?? 0)))
                 .ToList()))
            .ToList();
    }

    /// <summary>Grupperar lektionstyper till en behörighet/utbildningskategori för publikt filter.</summary>
    public static string BehorighetFor(int lessonTypeId) => lessonTypeId switch
    {
        5 or 6  => "Riskutbildning",
        7 or 10 => "Prov (Trafikverket)",
        _       => "Personbil B",
    };

    /// <summary>Alla behörigheter i fast ordning (för filterknappar).</summary>
    public static readonly string[] Behorigheter =
        { "Personbil B", "Riskutbildning", "Prov (Trafikverket)" };
}

public record AvailableDay(DateOnly Date, IReadOnlyList<AvailableSlot> Slots);
public record AvailableSlot(DateTime Start, DateTime End, string LessonType, string Behorighet);
