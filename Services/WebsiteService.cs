using System.Text.Json;
using System.Text.Json.Serialization;
using BookingDemo.Models;

namespace BookingDemo.Services;

// ════════════════════════════════════════════════════════════════════════════
//  WebsiteService – data och inställningar för den publika körskole-hemsidan.
//
//  Redigerbar data (profil, sidor, startsida, utbildningar, nyheter, anställda,
//  tema) ligger i Content och sparas till JSON så att admin-ändringar överlever
//  omstart. Produkter och lediga tider hämtas från BookingService (demodata).
// ════════════════════════════════════════════════════════════════════════════
public class WebsiteService
{
    private readonly BookingService _booking;
    private readonly string _storePath;
    private readonly string _imageRoot;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public WebsiteContent Content { get; private set; }

    public WebsiteService(BookingService booking, IWebHostEnvironment env)
    {
        _booking   = booking;
        _storePath = Path.Combine(env.ContentRootPath, "App_Data", "website.json");
        _imageRoot = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "Img", "Webb");

        Content = Load() ?? BuildDefaults(_booking);
    }

    // ── Bekväma accessorer (samma API som tidigare) ───────────────────────────
    public SchoolProfile   School    => Content.School;
    public List<Employee>  Employees => Content.Employees;
    public WebsiteSettings Settings  => Content.Settings;
    public ThemeSettings   Theme     => Content.Theme;

    public IEnumerable<Employee> WebEmployees => Employees.Where(e => e.ShowOnWeb);

    // ── Persistens ────────────────────────────────────────────────────────────
    private WebsiteContent? Load()
    {
        try
        {
            if (!File.Exists(_storePath)) return null;
            var json = File.ReadAllText(_storePath);
            var content = JsonSerializer.Deserialize<WebsiteContent>(json, JsonOpts);
            if (content != null) Backfill(content);
            return content;
        }
        catch
        {
            return null; // trasig fil → fall tillbaka på defaults
        }
    }

    /// <summary>
    /// Fyller på med standardinnehåll om en sparad fil saknar de nyare
    /// startsektionerna (USP, steg, kurser) – så att mockdatan inte försvinner
    /// när en äldre website.json läses in.
    /// </summary>
    private void Backfill(WebsiteContent c)
    {
        var d = BuildDefaults(_booking).Settings;
        c.Settings.Usp     ??= new();
        c.Settings.Steps   ??= new();
        c.Settings.Courses ??= new();
        if (c.Settings.Usp.Count == 0)     c.Settings.Usp     = d.Usp;
        if (c.Settings.Steps.Count == 0)   c.Settings.Steps   = d.Steps;
        if (c.Settings.Courses.Count == 0) c.Settings.Courses = d.Courses;
        if (string.IsNullOrWhiteSpace(c.Settings.StepsHeading)) c.Settings.StepsHeading = d.StepsHeading;
    }

    /// <summary>Sparar all redigerbar data till JSON. Anropas av admin efter ändringar.</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_storePath)!);
            File.WriteAllText(_storePath, JsonSerializer.Serialize(Content, JsonOpts));
        }
        catch
        {
            // Skrivskyddat filsystem (t.ex. viss host): ändringar gäller då bara i minnet.
        }
    }

    /// <summary>Återställer all hemside-data till leveransvärdena.</summary>
    public void ResetToDefaults()
    {
        Content = BuildDefaults(_booking);
        Save();
    }

    // ── Bilduppladdning ────────────────────────────────────────────────────────
    /// <summary>
    /// Sparar en uppladdad bild under wwwroot/Img/Webb[/subfolder] och returnerar
    /// dess webbsökväg (t.ex. "/Img/Webb/hero/min-bild.jpg").
    /// </summary>
    public async Task<string> SaveImageAsync(Stream stream, string originalName, string? subfolder = null)
    {
        var dir = subfolder is null ? _imageRoot : Path.Combine(_imageRoot, subfolder);
        Directory.CreateDirectory(dir);

        var safe = SafeFileName(originalName);
        var path = Path.Combine(dir, safe);

        await using (var fs = File.Create(path))
            await stream.CopyToAsync(fs);

        var rel = subfolder is null ? safe : $"{subfolder}/{safe}";
        return $"/Img/Webb/{rel}";
    }

    private static string SafeFileName(string name)
    {
        var cleaned = new string(Path.GetFileName(name)
            .Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '-').ToArray());
        // Unik prefix så att inget skrivs över av misstag.
        return $"{DateTime.Now:yyyyMMdd-HHmmss}-{cleaned}";
    }

    // ── Uppslag ────────────────────────────────────────────────────────────────
    public EducationCard? GetEducation(string slug) =>
        Settings.EducationCards.FirstOrDefault(c => c.Slug == slug);

    public IReadOnlyList<NewsItem> NewsByDate =>
        Settings.News.OrderByDescending(n => n.Date).ToList();

    public NewsItem? GetNews(string slug) =>
        Settings.News.FirstOrDefault(n => n.Slug == slug);

    // ── Prislista: webbflaggade produkter från affärssystemet ─────────────────
    public IEnumerable<Article> WebProducts =>
        _booking.Articles.Where(a => Settings.WebFlaggedArticleIds.Contains(a.Id));

    /// <summary>Alla artiklar i affärssystemet (för webbflaggning i admin).</summary>
    public IReadOnlyList<Article> AllArticles => _booking.Articles;

    public static string ProductCategoryFor(Article a)
    {
        if (a.ArticleNumber.StartsWith("RISK", StringComparison.OrdinalIgnoreCase)) return "Riskutbildning";
        if (a.ArticleNumber.StartsWith("UPP", StringComparison.OrdinalIgnoreCase) ||
            a.ArticleNumber.StartsWith("TEO", StringComparison.OrdinalIgnoreCase))  return "Prov & avgifter";
        return "Körlektioner";
    }

    public static readonly string[] ProductCategories =
        { "Körlektioner", "Riskutbildning", "Prov & avgifter" };

    // ── Lediga tider: speglar körskolans kalender (riktig demodata) ───────────
    public IReadOnlyList<AvailableDay> GetAvailableSlots(int days = 14)
    {
        var from = DateTime.Today;
        var to   = from.AddDays(days);
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

    public static string BehorighetFor(int lessonTypeId) => lessonTypeId switch
    {
        5 or 6  => "Riskutbildning",
        7 or 10 => "Prov (Trafikverket)",
        _       => "Personbil B",
    };

    public static readonly string[] Behorigheter =
        { "Personbil B", "Riskutbildning", "Prov (Trafikverket)" };

    // ── Färgteman (presets) ─────────────────────────────────────────────────────
    public static readonly IReadOnlyList<ThemeSettings> Presets = new List<ThemeSettings>
    {
        new() { PresetName = "Klassisk blå",  BannerColor = "#0b4f8a", ButtonColor = "#c8511b", HeaderBg = "#ffffff", FooterBg = "#e9edf1" },
        new() { PresetName = "Skogsgrön",     BannerColor = "#1b5e34", ButtonColor = "#b8541a", HeaderBg = "#ffffff", FooterBg = "#e9edf1" },
        new() { PresetName = "Djup natt",     BannerColor = "#1f3a93", ButtonColor = "#e07b00", HeaderBg = "#0f1830", FooterBg = "#e9edf1" },
        new() { PresetName = "Vinröd",        BannerColor = "#8a1f3d", ButtonColor = "#2a7d6f", HeaderBg = "#ffffff", FooterBg = "#e9edf1" },
    };

    // ════════════════════════════════════════════════════════════════════════
    //  Leveransvärden (seed). Används första gången, eller vid återställning.
    // ════════════════════════════════════════════════════════════════════════
    private static WebsiteContent BuildDefaults(BookingService booking) => new()
    {
        School = new()
        {
            Name = "Vägvisarens Trafikskola", LogoInitials = "VT",
            // Loggor som mockas in – lägg filerna i wwwroot/Img/Webb/logo/.
            LogoImage = "/Img/Webb/logo/logo.svg",
            StrLogo   = "/Img/Webb/logo/str.svg",
            Tagline = "Din kompletta trafikskola i stan",
            Phone = "054-21 40 50", Email = "info@vagvisaren.se",
            VisitAddress = "Storgatan 12, 652 25 Karlstad",
            PostalAddress = "Box 1234, 651 11 Karlstad", OrgNumber = "556714-4356",
            OpeningHours = new()
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
        },
        Employees = new()
        {
            new() { Id = 1, Name = "Anna Lindgren", Role = "Trafiklärare & VD",    Initials = "AL", ShowOnWeb = true,  Bio = "Driver skolan sedan 2009 och brinner för trygg, modern körkortsutbildning." },
            new() { Id = 2, Name = "Johan Berg",    Role = "Trafiklärare (B, BE)", Initials = "JB", ShowOnWeb = true,  Bio = "Specialist på släp och landsvägskörning. Lugn och pedagogisk." },
            new() { Id = 3, Name = "Sara Ek",       Role = "Trafiklärare (A, AM)", Initials = "SE", ShowOnWeb = true,  Bio = "MC- och mopedutbildning. Trygg start för nya förare." },
            new() { Id = 4, Name = "Mehmet Demir",  Role = "Administration",       Initials = "MD", ShowOnWeb = false, Bio = "Sköter bokningar och fakturor – syns ej på webben." },
        },
        Theme = new(),
        Settings = new()
        {
            Layout = StartLayout.FocusNews,
            WebFlaggedArticleIds = booking.Articles.Select(a => a.Id).ToList(),
            PageVisibility = new()
            {
                { WebPageKey.Ehandel, true }, { WebPageKey.Kalender, true }, { WebPageKey.Prislista, true },
                { WebPageKey.Personal, true }, { WebPageKey.Nyheter, true }, { WebPageKey.Kontakt, true },
            },
            HeroHeading = "Ta körkort hos Vägvisarens Trafikskola",
            HeroIngress = "Vi utbildar för bil, släp, moped och MC – med erfarna lärare och en trygg, modern utbildning hela vägen till uppkörningen.",
            HeroImage = "/Img/Webb/michael-kahn-Hfyrh5vUhuY-unsplash.jpg",
            PrimaryCtaText = "Boka utbildning", PrimaryCtaHref = "/webb/kalender",
            SecondaryCtaText = "Kontakta oss",  SecondaryCtaHref = "/webb/kontakt",
            EducationCards = new()
            {
                new() { Title = "Personbil (B)", Description = "Manuell och automat. Hela vägen till uppkörning.",
                    Icon = "directions_car", Image = "/Img/Webb/art-markiv-zAm1sdicGXc-unsplash.jpg",
                    Slug = "personbil-b", PriceFrom = "Från 950 kr/lektion",
                    LongText = "B-körkortet ger dig friheten att köra personbil. Hos oss får du en utbildning anpassad efter din nivå – oavsett om du är nybörjare eller har kört förut. Du väljer manuell eller automat, och vi planerar upplägget tillsammans så att du är trygg hela vägen till uppkörningen.",
                    Highlights = new() { "Introduktionslektion och nivåbedömning", "Manuell eller automatväxlad bil", "Teori varvat med körning", "Förberedelse inför kunskapsprov och uppkörning" } },
                new() { Title = "Släp (BE/B96)", Description = "Kör lagligt med tyngre släp och husvagn.",
                    Icon = "rv_hookup", Image = "/Img/Webb/rolando-garrido-R4y4_dvpYXU-unsplash.jpg",
                    Slug = "slap-be-b96", PriceFrom = "Från 1 200 kr/lektion",
                    LongText = "Med B96 eller BE får du dra tyngre släp, husvagn eller hästtransport. Vi går igenom koppling, lastsäkring och backning med släp så att du känner dig säker även med last bakom bilen.",
                    Highlights = new() { "Koppling och lastsäkring", "Backning och manövrering med släp", "Anpassat för B96 eller fullt BE", "Förberedelse inför uppkörning" } },
                new() { Title = "Moped & EU-moped", Description = "AM-kurs för dig som vill ut på vägarna.",
                    Icon = "two_wheeler", Image = "/Img/Webb/martin-nano-yOcUZn6jILI-unsplash.jpg",
                    Slug = "moped-am", PriceFrom = "9 995 kr (helkurs)",
                    LongText = "AM-körkortet låter dig köra EU-moped från 15 års ålder. Vår kurs ger dig både teorin och den praktiska körningen du behöver för att klara provet och köra säkert i trafiken.",
                    Highlights = new() { "Teori om trafikregler och säkerhet", "Praktisk körning i trafikmiljö", "Kursmaterial ingår", "Små grupper" } },
                new() { Title = "Riskutbildning", Description = "Risk 1 och Risk 2 – obligatoriskt inför körkort.",
                    Icon = "warning", Image = "/Img/Webb/bas-peperzak-tyhpK_QelPo-unsplash.jpg",
                    Slug = "riskutbildning", PriceFrom = "Från 1 200 kr",
                    LongText = "Riskutbildningen är obligatorisk för B-körkort. Risk 1 handlar om alkohol, droger, trötthet och riskfaktorer. Risk 2 är den praktiska halkbanan där du tränar på att hantera svåra situationer.",
                    Highlights = new() { "Risk 1 – teori om riskfaktorer", "Risk 2 – praktisk halkbana", "Krävs innan uppkörning", "Boka flera tillfällen via kalendern" } },
            },
            News = new()
            {
                new() { Slug = "sommarkurser-2026", Title = "Sommarens intensivkurser är öppna",
                    Summary = "Ta körkortet snabbt i sommar – boka en intensivkurs med start i juli. Begränsat antal platser.",
                    Body = "Nu öppnar vi anmälan till sommarens intensivkurser. Under två till tre veckor kombinerar vi tät körträning med teori så att du kan ta körkortet på kortare tid. Kurserna passar dig som har möjlighet att fokusera helhjärtat under en period.\n\nVi har ett begränsat antal platser per kurs för att hålla hög kvalitet och individuell uppföljning. Boka tidigt för att säkra din plats – kontakta oss så hjälper vi dig att hitta rätt upplägg.",
                    Date = new DateOnly(2026, 6, 15), Image = "/Img/Webb/foto-k-FYhEP1UMfcY-unsplash.jpg" },
                new() { Slug = "nya-automatbilar", Title = "Nya automatbilar i flottan",
                    Summary = "Vi har utökat med två nya automatväxlade bilar för en bekvämare körupplevelse.",
                    Body = "Vi fortsätter att modernisera vår fordonsflotta och har nu tagit in två nya automatväxlade bilar. Automat är ett populärt val för många elever eftersom du kan lägga mer fokus på trafiken och mindre på växling.\n\nVälkommen att prova en lektion i någon av våra nya bilar – prata med din lärare om vad som passar dig bäst.",
                    Date = new DateOnly(2026, 5, 28), Image = "/Img/Webb/sara-kurfess-bHNip8GaOso-unsplash.jpg" },
                new() { Slug = "halkbana-extra-tider", Title = "Risk 2 (halkbana) – extra tillfällen",
                    Summary = "Vi lägger till fler tider för halkbana under hösten. Boka via kalendern.",
                    Body = "På grund av hög efterfrågan lägger vi till fler tillfällen för Risk 2 (halkbana) under hösten. Riskutbildning del 2 är obligatorisk innan du får göra uppkörning, så passa på att boka in den i god tid.\n\nDe nya tiderna syns i bokningskalendern. Filtrera på behörigheten ”Riskutbildning” för att hitta dem snabbt.",
                    Date = new DateOnly(2026, 5, 10), Image = "/Img/Webb/mira-kireeva-GXA2XiAsko8-unsplash.jpg" },
            },
            Usp = new()
            {
                new() { Icon = "verified",        Title = "Ansluten till STR",     Text = "Kvalitetssäkrad utbildning enligt STR:s krav." },
                new() { Icon = "school",          Title = "Erfarna lärare",        Text = "Behöriga trafiklärare med lång erfarenhet." },
                new() { Icon = "directions_car",  Title = "Moderna bilar",         Text = "Manuellt och automat – trygga, nya fordon." },
                new() { Icon = "thumb_up",        Title = "Hög andel godkända",    Text = "Vi förbereder dig hela vägen till uppkörningen." },
            },
            StepsHeading = "Så tar du körkort – fyra enkla steg",
            Steps = new()
            {
                new() { Title = "Boka introduktion", Text = "Vi går igenom dina mål och lägger upp en plan som passar dig." },
                new() { Title = "Övningskör",        Text = "Körlektioner i din takt, varvat med teori och riskutbildning." },
                new() { Title = "Risk 1 & 2",        Text = "Obligatorisk riskutbildning – halkbana och trafiksäkerhet." },
                new() { Title = "Prov & körkort",    Text = "Vi förbereder dig inför kunskapsprov och uppkörning." },
            },
            Courses = new()
            {
                new() { Date = new DateOnly(2026, 7, 2),  Name = "Moped AM",            Price = 9995,  Full = false },
                new() { Date = new DateOnly(2026, 7, 6),  Name = "Riskutbildning 2 MC", Price = 3800,  Full = true  },
                new() { Date = new DateOnly(2026, 7, 14), Name = "Handledarutbildning",  Price = 550,   Full = false },
                new() { Date = new DateOnly(2026, 8, 10), Name = "Intensivkurs B",       Price = 14900, Full = false },
            },
        },
    };
}

public record AvailableDay(DateOnly Date, IReadOnlyList<AvailableSlot> Slots);
public record AvailableSlot(DateTime Start, DateTime End, string LessonType, string Behorighet);
