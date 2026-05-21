using BookingDemo.Models;

namespace BookingDemo.Services;

/// <summary>
/// Kontrakt för admin-rapporteringens "AI-agent".
/// Två implementationer delar samma tool-uppsättning (<see cref="ReportTools"/>):
///   - <see cref="TestLlmReportAgent"/> – deterministisk mock, används utan API-nyckel
///   - <see cref="OpenAIReportAgent"/>  – riktig LLM med function-calling
/// Vald automatiskt i Program.cs baserat på om OpenAI:ApiKey finns.
/// </summary>
public interface IReportAgent
{
    /// <summary>Förslag som visas som chips i UI:t. Grupperas på kategori.</summary>
    IReadOnlyList<SuggestedQuestion> SuggestedQuestions { get; }

    /// <summary>Svarar på en fritext-fråga. Returnerar både text och strukturerad rapport.</summary>
    Task<AgentMessage> AskAsync(string question, CancellationToken ct = default);
}
