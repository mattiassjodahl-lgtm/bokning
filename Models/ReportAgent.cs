namespace BookingDemo.Models;

// ── Agent DTOs ────────────────────────────────────────────────────────────────
// Lättvikts-modeller för konversation mellan admin-användare och "AI-agent".
// Designade så att dagens MockReportAgent kan bytas mot en riktig LLM-koppling
// utan att UI-koden behöver ändras.

public enum AgentRole { User, Agent }

public class AgentMessage
{
    public AgentRole Role { get; set; }
    public string Text { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>Valfri strukturerad rapport som agenten returnerar (diagram, tabell osv).</summary>
    public AgentReport? Report { get; set; }
}

/// <summary>Strukturerat svar från agenten. En fråga kan producera flera block.</summary>
public class AgentReport
{
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<ReportBlock> Blocks { get; set; } = new();
}

public enum BlockKind { KeyFigures, BarChart, LineChart, Table }

/// <summary>Ett enskilt innehållsblock i en rapport.</summary>
public class ReportBlock
{
    public BlockKind Kind { get; set; }
    public string Heading { get; set; } = "";

    // Används av KeyFigures
    public List<KeyFigure> Figures { get; set; } = new();

    // Används av BarChart / LineChart
    public List<string> Categories { get; set; } = new();
    public List<ChartSeries> Series { get; set; } = new();

    // Används av Table
    public List<string> Columns { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
}

public class KeyFigure
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Trend { get; set; }   // t.ex. "+12% vs förra månaden"
}

public class ChartSeries
{
    public string Name { get; set; } = "";
    public List<double> Values { get; set; } = new();
}
