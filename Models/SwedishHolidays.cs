/// <summary>
/// Beräknar svenska allmänna helgdagar för ett givet år.
/// Täcker rörliga helgdagar (påsk, pingstdagen, midsommar, alla helgons dag)
/// samt fasta röda dagar.
/// </summary>
public static class SwedishHolidays
{
    /// Returnerar alla röda dagar för angivet år som en HashSet för O(1)-sökning.
    public static HashSet<DateOnly> GetHolidays(int year)
    {
        var easter = CalculateEaster(year);

        var holidays = new HashSet<DateOnly>
        {
            // ── Fasta helgdagar ───────────────────────────────────────────
            new(year, 1,  1),   // Nyårsdagen
            new(year, 1,  6),   // Trettondedag jul
            new(year, 5,  1),   // Första maj
            new(year, 6,  6),   // Nationaldagen
            new(year, 12, 25),  // Juldagen
            new(year, 12, 26),  // Annandag jul

            // ── Påskbaserade helgdagar ────────────────────────────────────
            easter.AddDays(-2),  // Långfredag
            easter,              // Påskdagen
            easter.AddDays(1),   // Annandag påsk
            easter.AddDays(39),  // Kristi himmelsfärdsdag
            easter.AddDays(49),  // Pingstdagen

            // ── Kalenderbaserade helgdagar ────────────────────────────────
            MidsummerEve(year),     // Midsommarafton  (fredag 19–25 jun)
            MidsummerEve(year).AddDays(1), // Midsommardagen (lördagen efter)
            AllSaintsDay(year),     // Alla helgons dag (lördag 31 okt – 6 nov)
        };

        return holidays;
    }

    /// Returnerar helgdagsnamnet (för tooltip), eller null om datumet inte är helgdag.
    public static string? GetName(DateOnly date)
    {
        var easter = CalculateEaster(date.Year);
        return date switch
        {
            _ when date == new DateOnly(date.Year, 1,  1) => "Nyårsdagen",
            _ when date == new DateOnly(date.Year, 1,  6) => "Trettondedag jul",
            _ when date == new DateOnly(date.Year, 5,  1) => "Första maj",
            _ when date == new DateOnly(date.Year, 6,  6) => "Nationaldagen",
            _ when date == new DateOnly(date.Year, 12, 25) => "Juldagen",
            _ when date == new DateOnly(date.Year, 12, 26) => "Annandag jul",
            _ when date == easter.AddDays(-2)  => "Långfredag",
            _ when date == easter              => "Påskdagen",
            _ when date == easter.AddDays(1)   => "Annandag påsk",
            _ when date == easter.AddDays(39)  => "Kristi himmelsfärdsdag",
            _ when date == easter.AddDays(49)  => "Pingstdagen",
            _ when date == MidsummerEve(date.Year)           => "Midsommarafton",
            _ when date == MidsummerEve(date.Year).AddDays(1) => "Midsommardagen",
            _ when date == AllSaintsDay(date.Year)           => "Alla helgons dag",
            _ => null,
        };
    }

    // ── Påsk: Anonym Gregoriansk algoritm ────────────────────────────────────

    private static DateOnly CalculateEaster(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day   = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateOnly(year, month, day);
    }

    // ── Midsommarafton: fredagen 19–25 juni ──────────────────────────────────

    private static DateOnly MidsummerEve(int year)
    {
        var jun19        = new DateOnly(year, 6, 19);
        int daysToFriday = ((int)DayOfWeek.Friday - (int)jun19.DayOfWeek + 7) % 7;
        return jun19.AddDays(daysToFriday);
    }

    // ── Alla helgons dag: lördagen 31 okt – 6 nov ────────────────────────────

    private static DateOnly AllSaintsDay(int year)
    {
        var oct31       = new DateOnly(year, 10, 31);
        int daysToSat   = ((int)DayOfWeek.Saturday - (int)oct31.DayOfWeek + 7) % 7;
        return oct31.AddDays(daysToSat);
    }
}
