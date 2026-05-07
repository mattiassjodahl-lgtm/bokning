using System.Text.Json;

namespace BookingDemo.Models;

/// <summary>
/// Beskrivning av en "tool" som agenten kan anropa. Designat för att kunna
/// serialiseras direkt till OpenAI:s function-calling-format, men används
/// även av <see cref="Services.TestLlmReportAgent"/> som mock-router.
///
/// Varje tool är ett rent funktionsanrop: in -&gt; JSON-args, ut -&gt; AgentMessage.
/// </summary>
public sealed class AgentTool
{
    /// <summary>snake_case-namn, t.ex. "get_revenue_overview". Måste vara unikt.</summary>
    public required string Name { get; init; }

    /// <summary>Beskrivning som LLM:n läser för att avgöra när toolet är relevant.</summary>
    public required string Description { get; init; }

    /// <summary>JSON Schema för parametrar (enligt OpenAI function-calling-spec).</summary>
    public required string ParametersSchema { get; init; }

    /// <summary>Kör verktyget. <paramref name="args"/> är toolets argument som JSON-objekt.</summary>
    public required Func<JsonElement, AgentMessage> Execute { get; init; }
}
