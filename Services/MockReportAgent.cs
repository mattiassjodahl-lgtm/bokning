using BookingDemo.Models;

namespace BookingDemo.Services;

/// <summary>
/// Simulerad AI-agent. Matchar nyckelord i frågan och returnerar en pre-byggd
/// rapport baserad på demo-data från <see cref="BookingService"/>.
///
/// LLM-HOOK: Här kopplas en riktig LLM in när vi är redo. Se <see cref="AskAsync"/>
/// där routningen sker. Själva rapport-byggandet (BuildRevenueReport m.fl.) kan
/// återanvändas som "tools" som LLM:n anropar.
/// </summary>
public class MockReportAgent : IReportAgent
{
    private readonly BookingService _booking;
    private readonly Random _rng = new(42); // deterministisk demo

    public MockReportAgent(BookingService booking) => _booking = booking;

    public IReadOnlyList<string> SuggestedQuestions { get; } = new[]
    {
        "Hur mycket har vi sålt i år?",
        "Visa intäkter per månad senaste halvåret",
        "Vilken lärare omsätter mest?",
        "Hur ser beläggningen ut kommande vecka?",
        "Vilka tider är mest bokade?",
        "Hur full är beläggningen per lärare?",
    };

    public Task<AgentMessage> AskAsync(string question, CancellationToken ct = default)
    {
        var q = (question ?? "").ToLowerInvariant();

        // ── LLM-HOOK ──────────────────────────────────────────────────────────
        // När vi kopplar in en riktig LLM byts routingen nedan mot ett anrop
        // typ: var result = await _llm.CompleteAsync(q, tools: [BuildRevenueReport, BuildOccupancyReport, ...]);
        // Tools-metoderna nedan (Build*) fungerar redan som återanvändbara funktioner.
        // ─────────────────────────────────────────────────────────────────────

        AgentMessage reply;

        if (ContainsAny(q, "intäkt", "intakt", "försäljning", "forsaljning", "sålt", "salt", "omsätt", "omsatt", "kronor", "sek"))
        {
            if (ContainsAny(q, "lärare", "larare", "instruktör", "instruktor", "per lärare"))
                reply = BuildRevenuePerTeacherReport();
            else if (ContainsAny(q, "månad", "manad", "halvår", "halvar", "år", "ar"))
                reply = BuildRevenuePerMonthReport();
            else
                reply = BuildRevenueOverviewReport();
        }
        else if (ContainsAny(q, "bel", "boka", "schema", "ledig", "full"))
        {
            if (ContainsAny(q, "tid", "klockan", "timme", "timma"))
                reply = BuildOccupancyByHourReport();
            else if (ContainsAny(q, "lärare", "larare", "instruktör", "instruktor"))
                reply = BuildOccupancyPerTeacherReport();
            else
                reply = BuildOccupancyWeekReport();
        }
        else
        {
            reply = BuildFallback(question);
        }

        return Task.FromResult(reply);
    }

    // ── Tools (anropas av router – eller av en framtida LLM) ──────────────────

    private AgentMessage BuildRevenueOverviewReport()
    {
        var now = DateTime.Now;
        var ytdRevenue = 2_450_000m + _rng.Next(-50_000, 50_000);
        var lastYear = 2_180_000m;
        var growth = (ytdRevenue - lastYear) / lastYear * 100m;

        var months = Last6MonthLabels(now);
        var revenueSeries = new[] { 310_000.0, 340_000, 355_000, 370_000, 395_000, 420_000 };

        var articleBreakdown = new (string Name, decimal Sum)[]
        {
            ("Körlektion B, 60 min",  1_120_000m),
            ("Körlektion B, 90 min",    680_000m),
            ("Riskutbildning 1+2",      310_000m),
            ("Motorvägslektion",        220_000m),
            ("Intro / övrigt",          120_000m),
        };

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"Hittills i år har körskolan omsatt **{ytdRevenue:N0} kr**, en ökning på **{growth:F1}%** jämfört med samma period förra året. Körlektion B 60 min är den största intäktskällan.",
            Report = new AgentReport
            {
                Title = "Försäljning – översikt",
                Summary = $"År {now.Year} till och med {now:d MMMM}.",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Omsättning i år", Value = $"{ytdRevenue:N0} kr", Trend = $"+{growth:F1}% mot ifjol" },
                            new() { Label = "Snitt per månad",  Value = $"{(ytdRevenue / now.Month):N0} kr" },
                            new() { Label = "Aktiva elever",    Value = "186", Trend = "+14 denna månad" },
                        },
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.LineChart,
                        Heading = "Omsättning senaste 6 månaderna",
                        Categories = months,
                        Series =
                        {
                            new ChartSeries { Name = "Omsättning (kr)", Values = revenueSeries.ToList() }
                        },
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Heading = "Fördelning per artikel (i år)",
                        Columns = new() { "Artikel", "Omsättning", "Andel" },
                        Rows = articleBreakdown.Select(a => new List<string>
                        {
                            a.Name,
                            $"{a.Sum:N0} kr",
                            $"{(a.Sum / articleBreakdown.Sum(x => x.Sum)) * 100m:F1}%",
                        }).ToList(),
                    },
                }
            }
        };
    }

    private AgentMessage BuildRevenuePerMonthReport()
    {
        var months = Last6MonthLabels(DateTime.Now);
        var revenue = new[] { 310_000.0, 340_000, 355_000, 370_000, 395_000, 420_000 };
        var lessons = new[] { 380.0, 410, 425, 445, 470, 495 };

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = "Intäkterna har växt varje månad det senaste halvåret. April är starkast hittills – +35% jämfört med november.",
            Report = new AgentReport
            {
                Title = "Intäkter per månad",
                Summary = "Senaste sex månaderna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Omsättning per månad (kr)",
                        Categories = months,
                        Series = { new ChartSeries { Name = "Omsättning", Values = revenue.ToList() } },
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Månad", "Omsättning", "Antal lektioner" },
                        Rows = months.Select((m, i) => new List<string>
                        {
                            m, $"{revenue[i]:N0} kr", $"{lessons[i]:N0}"
                        }).ToList(),
                    },
                }
            }
        };
    }

    private AgentMessage BuildRevenuePerTeacherReport()
    {
        var teachers = _booking.Teachers
            .Where(t => t.IsSelected || t.Group == 1)
            .Take(6)
            .ToList();

        var values = new[] { 480_000.0, 440_000, 395_000, 360_000, 320_000, 280_000 };
        var rows = teachers.Select((t, i) => new List<string>
        {
            t.Name,
            $"{values[Math.Min(i, values.Length - 1)]:N0} kr",
            $"{(int)(values[Math.Min(i, values.Length - 1)] / 950):N0}",
        }).ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{teachers.First().Name}** ligger i topp med cirka {values[0]:N0} kr i omsättning i år. De tre främsta står för nästan 60% av total försäljning.",
            Report = new AgentReport
            {
                Title = "Omsättning per lärare",
                Summary = "Ackumulerat innevarande år",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Omsättning per lärare (kr)",
                        Categories = teachers.Select(t => t.Name).ToList(),
                        Series = { new ChartSeries { Name = "Omsättning", Values = teachers.Select((_, i) => values[Math.Min(i, values.Length - 1)]).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Lärare", "Omsättning", "Ca antal lektioner" },
                        Rows = rows,
                    }
                }
            }
        };
    }

    private AgentMessage BuildOccupancyWeekReport()
    {
        var days = new[] { "Mån", "Tis", "Ons", "Tors", "Fre", "Lör" };
        var booked   = new[] { 42.0, 46, 44, 48, 39, 22 };
        var capacity = new[] { 50.0, 50, 50, 50, 45, 30 };

        var avg = booked.Sum() / capacity.Sum() * 100;

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"Beläggningen nästa vecka ligger på **{avg:F0}%** i snitt. Torsdag är mest belagd (96%), lördag har mest ledigt.",
            Report = new AgentReport
            {
                Title = "Beläggning nästa vecka",
                Summary = "Bokade lektioner vs kapacitet",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Snittbeläggning",  Value = $"{avg:F0}%" },
                            new() { Label = "Lediga pass",      Value = $"{(capacity.Sum() - booked.Sum()):F0}" },
                            new() { Label = "Bokade lektioner", Value = $"{booked.Sum():F0}" },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Bokade vs kapacitet per dag",
                        Categories = days.ToList(),
                        Series =
                        {
                            new ChartSeries { Name = "Bokade",    Values = booked.ToList() },
                            new ChartSeries { Name = "Kapacitet", Values = capacity.ToList() },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Dag", "Bokade", "Kapacitet", "Beläggning" },
                        Rows = days.Select((d, i) => new List<string>
                        {
                            d, $"{booked[i]:F0}", $"{capacity[i]:F0}", $"{booked[i]/capacity[i]*100:F0}%"
                        }).ToList()
                    }
                }
            }
        };
    }

    private AgentMessage BuildOccupancyByHourReport()
    {
        var hours = new[] { "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18" };
        var load  = new[] { 62.0, 88, 95, 90, 45, 72, 91, 96, 92, 80, 55 };

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = "De mest populära tiderna är **10:00, 15:00 och 16:00** med över 90% beläggning. Lunchtimmen (12:00) är enklast att boka.",
            Report = new AgentReport
            {
                Title = "Beläggning per timme",
                Summary = "Genomsnitt senaste 30 dagarna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Beläggningsgrad per tidpunkt (%)",
                        Categories = hours.ToList(),
                        Series = { new ChartSeries { Name = "Beläggning", Values = load.ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Tid", "Beläggning", "Kommentar" },
                        Rows = hours.Select((h, i) => new List<string>
                        {
                            $"{h}:00",
                            $"{load[i]:F0}%",
                            load[i] >= 90 ? "Mycket hög" : load[i] >= 70 ? "Hög" : load[i] >= 50 ? "Medel" : "Låg"
                        }).ToList()
                    }
                }
            }
        };
    }

    private AgentMessage BuildOccupancyPerTeacherReport()
    {
        var teachers = _booking.Teachers.Take(6).ToList();
        var load = new[] { 94.0, 91, 87, 82, 76, 68 };

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{teachers.First().Name}** är mest uppbokad med 94% beläggning. Snittet ligger på {load.Average():F0}%.",
            Report = new AgentReport
            {
                Title = "Beläggning per lärare",
                Summary = "Genomsnitt senaste 30 dagarna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Beläggningsgrad (%)",
                        Categories = teachers.Select(t => t.Name).ToList(),
                        Series = { new ChartSeries { Name = "Beläggning", Values = teachers.Select((_, i) => load[Math.Min(i, load.Length - 1)]).ToList() } }
                    }
                }
            }
        };
    }

    private AgentMessage BuildFallback(string question)
    {
        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"Jag kan svara på frågor om försäljning och beläggning. Prova något av förslagen nedan, eller ställ en fråga med ord som *intäkter*, *omsättning*, *beläggning* eller *bokningar*.",
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool ContainsAny(string s, params string[] needles) =>
        needles.Any(n => s.Contains(n, StringComparison.OrdinalIgnoreCase));

    private static List<string> Last6MonthLabels(DateTime from)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE");
        var list = new List<string>();
        for (int i = 5; i >= 0; i--)
            list.Add(from.AddMonths(-i).ToString("MMM", culture));
        return list;
    }
}
