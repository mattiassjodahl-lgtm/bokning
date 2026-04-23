using BookingDemo.Models;

namespace BookingDemo.Services;

/// <summary>
/// Kontrakt för admin-rapporteringens "AI-agent".
/// MockReportAgent implementerar detta via nyckelordsmatchning mot demo-data.
/// Kan senare bytas mot en LLM-baserad implementation utan UI-ändringar.
/// </summary>
public interface IReportAgent
{
    /// <summary>Förslag som visas som chips i UI:t innan användaren skrivit något.</summary>
    IReadOnlyList<string> SuggestedQuestions { get; }

    /// <summary>Svarar på en fritext-fråga. Returnerar både text och strukturerad rapport.</summary>
    Task<AgentMessage> AskAsync(string question, CancellationToken ct = default);
}
