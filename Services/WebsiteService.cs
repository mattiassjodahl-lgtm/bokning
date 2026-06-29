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
            new() { Title = "Personbil (B)",   Description = "Manuell och automat. Hela vägen till uppkörning.", Icon = "directions_car", Image = "/Img/Webb/art-markiv-zAm1sdicGXc-unsplash.jpg" },
            new() { Title = "Släp (BE/B96)",   Description = "Kör lagligt med tyngre släp och husvagn.",         Icon = "rv_hookup",      Image = "/Img/Webb/rolando-garrido-R4y4_dvpYXU-unsplash.jpg" },
            new() { Title = "Moped & EU-moped", Description = "AM-kurs för dig som vill ut på vägarna.",          Icon = "two_wheeler",    Image = "/Img/Webb/martin-nano-yOcUZn6jILI-unsplash.jpg" },
            new() { Title = "Riskutbildning",  Description = "Risk 1 och Risk 2 – obligatoriskt inför körkort.",  Icon = "warning",        Image = "/Img/Webb/bas-peperzak-tyhpK_QelPo-unsplash.jpg" },
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

    // ── Prislista: webbflaggade produkter från affärssystemet ─────────────────
    public IEnumerable<Article> WebProducts =>
        _booking.Articles.Where(a => Settings.WebFlaggedArticleIds.Contains(a.Id));

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
