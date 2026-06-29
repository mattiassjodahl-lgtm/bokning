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
    Kontakt,
}

/// <summary>Körskolans profil – visas i header, footer och kontaktsida.</summary>
public class SchoolProfile
{
    public string Name        { get; set; } = "";
    /// <summary>Initialer som visas i logotyp-platshållaren (ersätts av uppladdad logga).</summary>
    public string LogoInitials { get; set; } = "";
    public string Tagline     { get; set; } = "";
    public string Phone       { get; set; } = "";
    public string Email       { get; set; } = "";
    public string VisitAddress { get; set; } = "";
    public string PostalAddress { get; set; } = "";
    public string OrgNumber   { get; set; } = "";
    /// <summary>Öppettider, en rad per intervall, t.ex. ("Måndag–Torsdag", "08.00–16.30").</summary>
    public List<OpeningHour> OpeningHours { get; set; } = new();
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

/// <summary>Nyhet som visas i Layout A:s nyhetsblock.</summary>
public class NewsItem
{
    public string Title { get; set; } = "";
    public string Body  { get; set; } = "";
    public DateOnly Date { get; set; }
}

/// <summary>Utbildningskort i hero-sektionen (start­sidan).</summary>
public class EducationCard
{
    public string Title       { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Material Icons-namn (samma uppsättning som affärssystemet använder).</summary>
    public string Icon        { get; set; } = "directions_car";
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
    public string PrimaryCtaText { get; set; } = "";
    public string PrimaryCtaHref { get; set; } = "";
    /// <summary>Andra CTA – endast obligatorisk i Layout B.</summary>
    public string SecondaryCtaText { get; set; } = "";
    public string SecondaryCtaHref { get; set; } = "";

    public List<EducationCard> EducationCards { get; set; } = new();
    public List<NewsItem>      News           { get; set; } = new();

    /// <summary>Artikel-ID:n (från affärssystemets PIM) som är webbflaggade → visas i prislistan.</summary>
    public List<int> WebFlaggedArticleIds { get; set; } = new();

    public bool IsVisible(WebPageKey key) =>
        key == WebPageKey.Start || PageVisibility.GetValueOrDefault(key, false);
}
