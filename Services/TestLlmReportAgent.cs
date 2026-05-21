using System.Text.Json;
using BookingDemo.Models;

namespace BookingDemo.Services;

/// <summary>
/// "Test-LLM": deterministisk router som väljer rätt tool och argument
/// baserat på frågan – utan att ringa något externt API.
///
/// Används som fallback när OpenAI-nyckel saknas, så demon alltid funkar.
/// Samma tool-uppsättning som <see cref="OpenAIReportAgent"/>, så när
/// kunden lägger till en API-nyckel byts beteendet ut utan UI-ändringar.
/// </summary>
public class TestLlmReportAgent : IReportAgent
{
    private readonly IReadOnlyList<AgentTool> _tools;
    private readonly Dictionary<string, AgentTool> _byName;

    public TestLlmReportAgent(IReadOnlyList<AgentTool> tools)
    {
        _tools = tools;
        _byName = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<SuggestedQuestion> SuggestedQuestions { get; } = new SuggestedQuestion[]
    {
        new("Ekonomi", "Hur mycket har vi sålt i år?"),
        new("Ekonomi", "Visa intäkter per månad senaste halvåret"),
        new("Ekonomi", "Visa omsättning senaste 12 månaderna med prognos"),
        new("Ekonomi", "Vilken lektionstyp är mest lönsam?"),
        new("Ekonomi", "Visa kostnader och marginal per lärare"),

        new("Beläggning", "Hur ser beläggningen ut kommande vecka?"),
        new("Beläggning", "Vilka tider är mest bokade?"),
        new("Beläggning", "Hur full är beläggningen per lärare?"),

        new("Lärare", "Vilken lärare omsätter mest?"),

        new("Elever", "Hur många nya elever har vi fått i år?"),
        new("Elever", "Vad är godkännandegraden på uppkörningen?"),

        new("Avbokningar", "Hur ser avbokningsfrekvensen ut?"),

        // Kombo-frågor (kör flera tools på en gång):
        new("Kombinerade", "Visa omsättning och beläggning för året"),
        new("Kombinerade", "Hur är elevstatistik och avbokningsfrekvens?"),
    };

    // Separatorer som indikerar att frågan kombinerar flera intents.
    private static readonly string[] _splitters = { " och ", " samt ", " plus ", " + ", " & " };

    public Task<AgentMessage> AskAsync(string question, CancellationToken ct = default)
    {
        var q = question ?? "";

        // Detektera kombo-frågor: splitta på "och"/"samt"/"+" och kör flera tools
        var parts = SplitOnCombiners(q);
        var picks = new List<(string Tool, string Args)>();
        var seen = new HashSet<string>();

        foreach (var part in parts)
        {
            var (toolName, args) = SelectTool(part);
            if (toolName is null) continue;
            if (seen.Add(toolName))
                picks.Add((toolName, args));
        }

        if (picks.Count == 0)
            return Task.FromResult(BuildFallback());

        var results = picks
            .Where(p => _byName.ContainsKey(p.Tool))
            .Select(p => _byName[p.Tool].Execute(JsonDocument.Parse(p.Args).RootElement))
            .ToList();

        return Task.FromResult(ReportTools.Combine(results));
    }

    private static List<string> SplitOnCombiners(string question)
    {
        var lower = question.ToLowerInvariant();
        // Snabbcheck – innehåller frågan ens en kombinerare?
        if (!_splitters.Any(s => lower.Contains(s)))
            return new List<string> { question };

        // Splitta sekventiellt på alla separatorer
        var current = new List<string> { question };
        foreach (var sep in _splitters)
        {
            current = current
                .SelectMany(p => p.Split(sep, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .ToList();
        }
        return current;
    }

    /// <summary>
    /// Simulerar LLM:s val av tool baserat på frågans nyckelord.
    /// Returnerar tool-namn och argument som JSON-objekt.
    /// </summary>
    private (string? Tool, string Args) SelectTool(string question)
    {
        var q = question.ToLowerInvariant();

        if (ContainsAny(q, "avbok", "no-show", "no show", "uteblev", "uteblivna", "ombok"))
            return ("get_cancellations", "{}");

        if (ContainsAny(q, "marginal", "lönsam", "lonsam", "vinst", "kostnad", "netto", "brutto"))
        {
            return ContainsAny(q, "lärare", "larare", "instruktör", "instruktor")
                ? ("get_margin_per_teacher", "{}")
                : ("get_margin_per_lesson_type", "{}");
        }

        if (ContainsAny(q, "elev", "uppkörning", "uppkorning", "teoriprov", "godkänn", "godkann", "körkort", "korkort"))
            return ("get_student_stats", "{}");

        if (ContainsAny(q, "intäkt", "intakt", "försäljning", "forsaljning", "sålt", "salt", "omsätt", "omsatt", "kronor", "sek"))
        {
            if (ContainsAny(q, "12 mån", "12 manad", "helår", "helar", "senaste året", "senaste aret", "prognos", "förra året", "forra aret", "yoy"))
                return ("get_year_overview", "{}");
            if (ContainsAny(q, "lärare", "larare", "instruktör", "instruktor"))
                return ("get_revenue_per_teacher", "{}");
            if (ContainsAny(q, "månad", "manad", "halvår", "halvar", "år", "ar"))
            {
                var months = ContainsAny(q, "12", "år", "helår", "ar", "helar") ? 12 : 6;
                return ("get_revenue_per_month", $"{{\"months\":{months}}}");
            }
            return ("get_revenue_overview", "{}");
        }

        if (ContainsAny(q, "bel", "boka", "schema", "ledig", "full"))
        {
            if (ContainsAny(q, "tid", "klockan", "timme", "timma"))
                return ("get_occupancy_by_hour", "{}");
            if (ContainsAny(q, "lärare", "larare", "instruktör", "instruktor"))
                return ("get_occupancy_per_teacher", "{}");
            return ("get_occupancy_week", "{}");
        }

        return (null, "{}");
    }

    private AgentMessage BuildFallback() => new()
    {
        Role = AgentRole.Agent,
        Text = "Jag kan svara på frågor om försäljning, beläggning, elever, avbokningar och marginal. Prova något av förslagen nedan, eller använd ord som *omsättning*, *prognos*, *elever*, *uppkörning*, *avbokningar* eller *marginal*.",
    };

    private static bool ContainsAny(string s, params string[] needles) =>
        needles.Any(n => s.Contains(n, StringComparison.OrdinalIgnoreCase));
}
