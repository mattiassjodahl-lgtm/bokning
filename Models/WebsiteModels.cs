namespace BookingDemo.Models;

// ════════════════════════════════════════════════════════════════════════════
//  Modeller för den publika körskole-hemsidan (STR e-handel/hemsida).
//  All data mockas i WebsiteService. I produktion kommer detta från
//  affärssystemet (körskolans profil, anställda, webbflaggade produkter osv).
// ════════════════════════════════════════════════════════════════════════════

/// <summary>De två valbara layouterna för startsidan. Övriga sidor är fasta.</summary>
public enum StartLayout
{
    /// <summary>Layout A – hero + utbildningskort + nyhetsblock + kommande kurser.</summary>
    FocusNews,
    /// <summary>Layout B – hero med dubbla CTA + brett utbildningsgrid + kommande kurser.</summary>
    BroadAction,
}

/// <summary>Identifierar varje sida/menylänk på hemsidan.</summary>
public enum WebPageKey
{
    Start,
    Ehandel,
    Kalender,
    Prislista,
    Personal,
    Nyheter,
    Kontakt,
}

/// <summary>Körskolans profil – visas i header, footer och kontaktsida.</summary>
public class SchoolProfile
{
    public string Name        { get; set; } = "";
    /// <summary>Initialer som visas i logotyp-platshållaren om ingen logga laddats upp.</summary>
    public string LogoInitials { get; set; } = "";
    /// <summary>Sökväg till uppladdad logga under wwwroot (SVG/vektor eller PNG). Tom = visa initialer.</summary>
    public string LogoImage   { get; set; } = "";
    public string Tagline     { get; set; } = "";
    public string Phone       { get; set; } = "";
    public string Email       { get; set; } = "";
    public string VisitAddress { get; set; } = "";
    public string PostalAddress { get; set; } = "";
    public string OrgNumber   { get; set; } = "";
    /// <summary>Öppettider, en rad per intervall, t.ex. ("Måndag–Torsdag", "08.00–16.30").</summary>
    public List<OpeningHour> OpeningHours { get; set; } = new();
    /// <summary>Sociala medier-länkar som visas i footern.</summary>
    public List<SocialLink> Social { get; set; } = new();

    /// <summary>Länk till STR (visas i "Medlem i"-blocket och menyn).</summary>
    public string StrUrl  { get; set; } = "https://www.str.se";
    /// <summary>Uppladdad STR-logga under wwwroot. Tom = textmärke "STR".</summary>
    public string StrLogo { get; set; } = "";
}

/// <summary>En länk till ett socialt medium. Platform styr vilken ikon som visas.</summary>
public class SocialLink
{
    /// <summary>Nyckel för ikonen: "facebook", "instagram" eller "youtube".</summary>
    public string Platform { get; set; } = "";
    /// <summary>Visningsnamn för skärmläsare, t.ex. "Facebook".</summary>
    public string Label    { get; set; } = "";
    public string Url      { get; set; } = "#";
}

public class OpeningHour
{
    public string Days { get; set; } = "";
    public string Hours { get; set; } = "";
}

/// <summary>En anställd. Visas på personalsidan endast om ShowOnWeb = true.</summary>
public class Employee
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = "";
    public string Role      { get; set; } = "";
    public string Bio       { get; set; } = "";
    /// <summary>Styrs i affärssystemet (mockas här). False = döljs på webben.</summary>
    public bool   ShowOnWeb { get; set; } = true;
    /// <summary>Initialer för avatar-platshållare tills foto laddats upp.</summary>
    public string Initials  { get; set; } = "";
}

/// <summary>Nyhet – visas i startsidans nyhetsblock, i listningen och på detaljsidan.</summary>
public class NewsItem
{
    /// <summary>URL-segment, t.ex. "sommarkurser-2026".</summary>
    public string Slug    { get; set; } = "";
    public string Title   { get; set; } = "";
    /// <summary>Kort ingress för kort/listning.</summary>
    public string Summary { get; set; } = "";
    /// <summary>Hela brödtexten på detaljsidan.</summary>
    public string Body    { get; set; } = "";
    public DateOnly Date  { get; set; }
    /// <summary>Sökväg till bild under wwwroot. Tom = ingen bild.</summary>
    public string Image   { get; set; } = "";
    /// <summary>Alt-text för bilden. Tom = faller tillbaka på rubriken.</summary>
    public string ImageAlt { get; set; } = "";
}

/// <summary>Utbildningskort på startsidan + innehåll för dess behörighetssida.</summary>
public class EducationCard
{
    public string Title       { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Material Icons-namn (visas om Image saknas).</summary>
    public string Icon        { get; set; } = "directions_car";
    /// <summary>Sökväg till kort-bild under wwwroot, t.ex. "/Img/Webb/bil.jpg". Tom = visa ikon.</summary>
    public string Image       { get; set; } = "";
    /// <summary>Alt-text för bilden. Tom = faller tillbaka på titeln.</summary>
    public string ImageAlt    { get; set; } = "";

    // ── Behörighetssida (/webb/utbildning/{Slug}) ─────────────────────────────
    /// <summary>URL-segment, t.ex. "personbil-b". Tomt = ingen egen sida.</summary>
    public string Slug        { get; set; } = "";
    /// <summary>Längre brödtext på behörighetssidan.</summary>
    public string LongText    { get; set; } = "";
    /// <summary>Punktlista med vad utbildningen innehåller.</summary>
    public List<string> Highlights { get; set; } = new();
    /// <summary>Prisindikation, t.ex. "Från 950 kr/lektion". Tomt = visas ej.</summary>
    public string PriceFrom   { get; set; } = "";

    public bool HasPage => !string.IsNullOrEmpty(Slug);
}

/// <summary>Säljpunkt i USP-bandet på startsidan.</summary>
public class UspItem
{
    /// <summary>Material Icons-namn, t.ex. "verified".</summary>
    public string Icon  { get; set; } = "verified";
    public string Title { get; set; } = "";
    public string Text  { get; set; } = "";
}

/// <summary>Ett steg i "Så tar du körkort"-guiden.</summary>
public class StepItem
{
    public string Title { get; set; } = "";
    public string Text  { get; set; } = "";
}

/// <summary>En rad i "Kommande kurser"-tabellen på startsidan.</summary>
public class CourseItem
{
    public DateOnly Date { get; set; }
    public string   Name { get; set; } = "";
    public decimal  Price { get; set; }
    /// <summary>True = fullbokad (visas som "Fullt").</summary>
    public bool     Full { get; set; }
}

/// <summary>Innehåll och inställningar för hela hemsidan (en körskola).</summary>
public class WebsiteSettings
{
    public StartLayout Layout { get; set; } = StartLayout.FocusNews;

    /// <summary>Vilka sidor som är synliga i menyn. Start är alltid synlig.</summary>
    public Dictionary<WebPageKey, bool> PageVisibility { get; set; } = new();

    // ── Startsidans redigerbara innehåll ──────────────────────────────────────
    public string HeroHeading { get; set; } = "";
    public string HeroIngress { get; set; } = "";
    /// <summary>Hero-bild under wwwroot, t.ex. "/Img/Webb/hero.jpg". Tom = enbart gradient.</summary>
    public string HeroImage   { get; set; } = "";
    public string PrimaryCtaText { get; set; } = "";
    public string PrimaryCtaHref { get; set; } = "";
    /// <summary>Andra CTA – endast obligatorisk i Layout B.</summary>
    public string SecondaryCtaText { get; set; } = "";
    public string SecondaryCtaHref { get; set; } = "";

    public List<EducationCard> EducationCards { get; set; } = new();
    public List<NewsItem>      News           { get; set; } = new();

    // ── Startsidans sektioner (på/av + innehåll) ──────────────────────────────
    public bool ShowUsp     { get; set; } = true;
    public bool ShowSteps   { get; set; } = true;
    public bool ShowNews    { get; set; } = true;
    public bool ShowCourses { get; set; } = true;

    public List<UspItem>   Usp          { get; set; } = new();
    public string          StepsHeading { get; set; } = "Så tar du körkort – fyra enkla steg";
    public List<StepItem>  Steps        { get; set; } = new();
    public List<CourseItem> Courses     { get; set; } = new();

    /// <summary>Artikel-ID:n (från affärssystemets PIM) som är webbflaggade → visas i prislistan.</summary>
    public List<int> WebFlaggedArticleIds { get; set; } = new();

    public bool IsVisible(WebPageKey key) =>
        key == WebPageKey.Start || PageVisibility.GetValueOrDefault(key, false);
}

/// <summary>Färgtema för hemsidan. Mappas till CSS-variabler i PublicLayout.</summary>
public class ThemeSettings
{
    /// <summary>Namn på valt preset (eller "Egen" vid finjustering).</summary>
    public string PresetName  { get; set; } = "Klassisk blå";
    /// <summary>Primär-/bannerfärg – hero, länkar, aktiv meny.</summary>
    public string BannerColor { get; set; } = "#0b4f8a";
    /// <summary>Knapp-/CTA-färg.</summary>
    public string ButtonColor { get; set; } = "#c8511b";
    /// <summary>Bakgrundsfärg för header.</summary>
    public string HeaderBg    { get; set; } = "#ffffff";
    /// <summary>Bakgrundsfärg för footer (ljusgrå som default – bäst kontrast mot loggor).</summary>
    public string FooterBg    { get; set; } = "#e9edf1";
}

/// <summary>
/// All redigerbar hemside-data för en körskola. Serialiseras till JSON så att
/// admin-ändringar överlever omstart. Produkter/lediga tider kommer från
/// affärssystemet och ingår inte här.
/// </summary>
public class WebsiteContent
{
    public SchoolProfile   School    { get; set; } = new();
    public WebsiteSettings Settings  { get; set; } = new();
    public List<Employee>  Employees { get; set; } = new();
    public ThemeSettings   Theme     { get; set; } = new();
}
