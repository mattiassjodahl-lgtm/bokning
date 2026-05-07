using System.Text.Json;
using BookingDemo.Models;

namespace BookingDemo.Services;

/// <summary>
/// Central registry för rapport-tools. Anropas av både TestLlmReportAgent
/// och OpenAIReportAgent. Varje tool är en ren funktion: JSON-args -&gt; AgentMessage.
///
/// Lägg till nya rapporter genom att lägga till en post i <see cref="Create"/>.
/// </summary>
public static class ReportTools
{
    private static readonly Random _rng = new(42); // deterministisk demo

    /// <summary>Bygger hela toolset:et. Kallas en gång vid app-start och delas mellan agenter.</summary>
    public static IReadOnlyList<AgentTool> Create(BookingService booking)
    {
        return new[]
        {
            new AgentTool
            {
                Name = "get_revenue_overview",
                Description = "Översikt över årets omsättning hittills, fördelat per artikel/lektionstyp. Ger nyckeltal, månadstrend och tabell.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildRevenueOverviewReport(),
            },
            new AgentTool
            {
                Name = "get_revenue_per_month",
                Description = "Omsättning per månad. Använd månadsparametern för att få halvår eller helår.",
                ParametersSchema = """
                {
                  "type": "object",
                  "properties": {
                    "months": { "type": "integer", "enum": [6, 12], "description": "Antal månader bakåt. 6 eller 12." }
                  },
                  "required": []
                }
                """,
                Execute = args => BuildRevenuePerMonthReport(GetInt(args, "months", 6)),
            },
            new AgentTool
            {
                Name = "get_revenue_per_teacher",
                Description = "Omsättning per lärare för innevarande år, sorterat fallande.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildRevenuePerTeacherReport(booking),
            },
            new AgentTool
            {
                Name = "get_year_overview",
                Description = "Helår: rullande 12 månaders omsättning, jämförelse mot föregående år (YoY) och prognos kommande månad.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildYearOverviewReport(),
            },
            new AgentTool
            {
                Name = "get_occupancy_week",
                Description = "Beläggning för nästa vecka: bokade lektioner vs kapacitet per dag.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildOccupancyWeekReport(),
            },
            new AgentTool
            {
                Name = "get_occupancy_by_hour",
                Description = "Beläggning per tidpunkt på dygnet (genomsnitt senaste 30 dagarna).",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildOccupancyByHourReport(),
            },
            new AgentTool
            {
                Name = "get_occupancy_per_teacher",
                Description = "Beläggning per lärare. Skicka teacher_name för att fokusera på en specifik lärare.",
                ParametersSchema = """
                {
                  "type": "object",
                  "properties": {
                    "teacher_name": { "type": "string", "description": "Förnamn eller fullständigt namn på en lärare. Tom = alla." }
                  },
                  "required": []
                }
                """,
                Execute = args => BuildOccupancyPerTeacherReport(booking, GetString(args, "teacher_name")),
            },
            new AgentTool
            {
                Name = "get_student_stats",
                Description = "Elev-statistik: aktiva elever, nya elever per månad, klara körkort, godkännandegrad teori/uppkörning, snittid.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildStudentStatsReport(),
            },
            new AgentTool
            {
                Name = "get_cancellations",
                Description = "Avbokningar och no-show: frekvens, trend, förlorad intäkt och no-show per lärare.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildCancellationReport(booking),
            },
            new AgentTool
            {
                Name = "get_margin_per_lesson_type",
                Description = "Bruttomarginal per lektionstyp: pris, kostnad, marginal i kr och procent.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildMarginPerLessonTypeReport(),
            },
            new AgentTool
            {
                Name = "get_margin_per_teacher",
                Description = "Nettomarginal per lärare (intäkt minus lönekostnad och rörlig fordonskostnad).",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildMarginPerTeacherReport(booking),
            },
            new AgentTool
            {
                Name = "get_resource_inventory",
                Description = "Resursinventarie: alla bilar, motorcyklar, släp, lektionssalar och simulator – med status, årsmodell, miltal och kapacitet.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildResourceInventoryReport(booking),
            },
            new AgentTool
            {
                Name = "get_resource_utilization",
                Description = "Beläggning per resurs (bilar, MC, släp, salar) senaste 30 dagarna, sorterat fallande.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildResourceUtilizationReport(booking),
            },
            new AgentTool
            {
                Name = "get_resource_utilization_by_type",
                Description = "Beläggning aggregerat per resurstyp: Bil, Motorcykel, Släp, Sal, Simulator.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildResourceUtilizationByTypeReport(booking),
            },
            new AgentTool
            {
                Name = "get_resource_costs",
                Description = "Driftkostnader och kommande service per fordon. Visar rörlig kostnad, körda timmar och nästa servicetillfälle.",
                ParametersSchema = EmptyObjectSchema,
                Execute = _ => BuildResourceCostsReport(booking),
            },
        };
    }

    // ── Build-metoder (samma logik som tidigare, nu rena funktioner) ──────────

    private static AgentMessage BuildRevenueOverviewReport()
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
                            new() { Label = "Snitt per månad", Value = $"{(ytdRevenue / now.Month):N0} kr" },
                            new() { Label = "Aktiva elever",   Value = "186", Trend = "+14 denna månad" },
                        },
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.LineChart,
                        Heading = "Omsättning senaste 6 månaderna",
                        Categories = months,
                        Series = { new ChartSeries { Name = "Omsättning (kr)", Values = revenueSeries.ToList() } },
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

    private static AgentMessage BuildRevenuePerMonthReport(int months)
    {
        months = months == 12 ? 12 : 6;
        var labels = months == 12 ? Last12MonthLabels(DateTime.Now) : Last6MonthLabels(DateTime.Now);
        var revenue6  = new[] { 310_000.0, 340_000, 355_000, 370_000, 395_000, 420_000 };
        var revenue12 = new[] { 295_000.0, 305_000, 280_000, 240_000, 195_000, 220_000,
                                330_000, 360_000, 345_000, 370_000, 395_000, 420_000 };
        var lessons6  = new[] { 380.0, 410, 425, 445, 470, 495 };
        var lessons12 = new[] { 360.0, 370, 340, 290, 240, 270, 400, 430, 415, 445, 470, 495 };

        var revenue = months == 12 ? revenue12 : revenue6;
        var lessons = months == 12 ? lessons12 : lessons6;

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = months == 12
                ? $"Senaste 12 månaderna har omsättningen vuxit. April är starkast med {revenue.Last():N0} kr."
                : "Intäkterna har växt varje månad det senaste halvåret. April är starkast hittills.",
            Report = new AgentReport
            {
                Title = $"Intäkter per månad ({months} mån)",
                Summary = months == 12 ? "Senaste 12 månaderna" : "Senaste 6 månaderna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Omsättning per månad (kr)",
                        Categories = labels,
                        Series = { new ChartSeries { Name = "Omsättning", Values = revenue.ToList() } },
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Månad", "Omsättning", "Antal lektioner" },
                        Rows = labels.Select((m, i) => new List<string>
                        {
                            m, $"{revenue[i]:N0} kr", $"{lessons[i]:N0}"
                        }).ToList(),
                    },
                }
            }
        };
    }

    private static AgentMessage BuildRevenuePerTeacherReport(BookingService booking)
    {
        var teachers = booking.Teachers.Where(t => t.IsSelected || t.Group == 1).Take(6).ToList();
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

    private static AgentMessage BuildYearOverviewReport()
    {
        var now = DateTime.Now;
        var months = Last12MonthLabels(now);
        var revenue  = new[] { 295_000.0, 305_000, 280_000, 240_000, 195_000, 220_000,
                               330_000, 360_000, 345_000, 370_000, 395_000, 420_000 };
        var lastYear = new[] { 270_000.0, 280_000, 260_000, 220_000, 175_000, 200_000,
                               300_000, 325_000, 310_000, 335_000, 355_000, 380_000 };

        var totalThis = revenue.Sum();
        var totalLast = lastYear.Sum();
        var growth = (totalThis - totalLast) / totalLast * 100.0;
        var forecast = revenue.Skip(revenue.Length - 3).Average() * 1.05;

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"Senaste 12 månaderna har körskolan omsatt **{totalThis:N0} kr** (+{growth:F1}% vs föregående 12 mån). Prognos kommande månad: **{forecast:N0} kr**.",
            Report = new AgentReport
            {
                Title = "Helår – omsättning & prognos",
                Summary = "Rullande 12 månader med jämförelse mot föregående år",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Omsättning 12 mån", Value = $"{totalThis:N0} kr", Trend = $"+{growth:F1}% YoY" },
                            new() { Label = "Snitt/månad",       Value = $"{(totalThis / 12):N0} kr" },
                            new() { Label = "Prognos nästa mån", Value = $"{forecast:N0} kr", Trend = "baserat på Q-trend" },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.LineChart,
                        Heading = "Omsättning per månad – i år vs ifjol",
                        Categories = months,
                        Series =
                        {
                            new ChartSeries { Name = "I år",  Values = revenue.ToList() },
                            new ChartSeries { Name = "Ifjol", Values = lastYear.ToList() },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Heading = "Detaljer per månad",
                        Columns = new() { "Månad", "I år", "Ifjol", "Diff" },
                        Rows = months.Select((m, i) => new List<string>
                        {
                            m,
                            $"{revenue[i]:N0} kr",
                            $"{lastYear[i]:N0} kr",
                            $"{(revenue[i] - lastYear[i]):+#,##0;-#,##0;0} kr",
                        }).ToList(),
                    }
                }
            }
        };
    }

    private static AgentMessage BuildOccupancyWeekReport()
    {
        var days     = new[] { "Mån", "Tis", "Ons", "Tors", "Fre", "Lör" };
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

    private static AgentMessage BuildOccupancyByHourReport()
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

    private static AgentMessage BuildOccupancyPerTeacherReport(BookingService booking, string? teacherName)
    {
        var allTeachers = booking.Teachers.Take(6).ToList();
        var load = new[] { 94.0, 91, 87, 82, 76, 68 };

        // Filtrera om teacher_name skickats med
        var teachers = string.IsNullOrWhiteSpace(teacherName)
            ? allTeachers
            : allTeachers.Where(t => t.Name.Contains(teacherName, StringComparison.OrdinalIgnoreCase)).ToList();

        if (teachers.Count == 0) teachers = allTeachers; // fallback

        var teacherLoad = teachers.Select((_, i) => load[Math.Min(i, load.Length - 1)]).ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = teachers.Count == 1
                ? $"**{teachers[0].Name}** har {teacherLoad[0]:F0}% beläggning senaste 30 dagarna."
                : $"**{teachers.First().Name}** är mest uppbokad med {teacherLoad[0]:F0}% beläggning. Snittet ligger på {teacherLoad.Average():F0}%.",
            Report = new AgentReport
            {
                Title = teachers.Count == 1 ? $"Beläggning – {teachers[0].Name}" : "Beläggning per lärare",
                Summary = "Genomsnitt senaste 30 dagarna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Beläggningsgrad (%)",
                        Categories = teachers.Select(t => t.Name).ToList(),
                        Series = { new ChartSeries { Name = "Beläggning", Values = teacherLoad } }
                    }
                }
            }
        };
    }

    private static AgentMessage BuildStudentStatsReport()
    {
        var months = Last6MonthLabels(DateTime.Now);
        var newStudents      = new[] { 22.0, 28, 31, 26, 34, 39 };
        var finishedLicenses = new[] { 14.0, 18, 17, 21, 19, 24 };

        const double passTheory = 78.4;
        const double passDriving = 64.2;
        const int avgDaysToLicense = 142;
        const int activeStudents = 186;

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{activeStudents} aktiva elever**. Senaste halvåret har vi tagit in **{newStudents.Sum():F0}** nya och **{finishedLicenses.Sum():F0}** har tagit körkort. Godkännandegrad uppkörning: **{passDriving:F1}%** (teori: {passTheory:F1}%).",
            Report = new AgentReport
            {
                Title = "Elev-statistik",
                Summary = "Senaste 6 månaderna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Aktiva elever",        Value = $"{activeStudents}", Trend = "+14 denna mån" },
                            new() { Label = "Godkänd uppkörning",   Value = $"{passDriving:F1}%", Trend = "+1.8 pp vs ifjol" },
                            new() { Label = "Godkänt teoriprov",    Value = $"{passTheory:F1}%",  Trend = "+0.6 pp vs ifjol" },
                            new() { Label = "Snittid till körkort", Value = $"{avgDaysToLicense} dgr" },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Nya elever vs klara körkort per månad",
                        Categories = months,
                        Series =
                        {
                            new ChartSeries { Name = "Nya elever",    Values = newStudents.ToList() },
                            new ChartSeries { Name = "Klara körkort", Values = finishedLicenses.ToList() },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Heading = "Detaljer per månad",
                        Columns = new() { "Månad", "Nya elever", "Klara körkort" },
                        Rows = months.Select((m, i) => new List<string>
                        {
                            m, $"{newStudents[i]:F0}", $"{finishedLicenses[i]:F0}"
                        }).ToList()
                    }
                }
            }
        };
    }

    private static AgentMessage BuildCancellationReport(BookingService booking)
    {
        var months = Last6MonthLabels(DateTime.Now);
        var cancellations = new[] { 38.0, 42, 35, 47, 41, 44 };
        var noShows       = new[] {  9.0, 12,  8, 14, 11, 13 };
        var totalLessons  = new[] { 380.0, 410, 425, 445, 470, 495 };

        var avgCancelRate = cancellations.Zip(totalLessons, (c, t) => c / t).Average() * 100;
        var avgNoShowRate = noShows.Zip(totalLessons, (n, t) => n / t).Average() * 100;

        var teachers = booking.Teachers.Take(5).ToList();
        var teacherNoShow = new[] { 1.2, 2.4, 3.1, 3.8, 4.5 };

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"Avbokningsfrekvensen ligger på **{avgCancelRate:F1}%** och no-show på **{avgNoShowRate:F1}%** i snitt senaste halvåret. Sena ombokningar (<24h) är vanligast på fredagar.",
            Report = new AgentReport
            {
                Title = "Avbokningar & no-show",
                Summary = "Senaste 6 månaderna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Avbokningar",     Value = $"{cancellations.Sum():F0}", Trend = $"{avgCancelRate:F1}% av lektioner" },
                            new() { Label = "No-show",         Value = $"{noShows.Sum():F0}",       Trend = $"{avgNoShowRate:F1}% av lektioner" },
                            new() { Label = "Förlorad intäkt", Value = $"{(noShows.Sum() * 950):N0} kr", Trend = "uppskattat" },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.LineChart,
                        Heading = "Trend – avbokningar och no-show",
                        Categories = months,
                        Series =
                        {
                            new ChartSeries { Name = "Avbokningar", Values = cancellations.ToList() },
                            new ChartSeries { Name = "No-show",     Values = noShows.ToList() },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Heading = "No-show per lärare (senaste 30 dagarna)",
                        Columns = new() { "Lärare", "No-show %", "Status" },
                        Rows = teachers.Select((t, i) => new List<string>
                        {
                            t.Name,
                            $"{teacherNoShow[Math.Min(i, teacherNoShow.Length - 1)]:F1}%",
                            teacherNoShow[Math.Min(i, teacherNoShow.Length - 1)] < 2 ? "Bra"
                                : teacherNoShow[Math.Min(i, teacherNoShow.Length - 1)] < 4 ? "OK" : "Bevaka",
                        }).ToList()
                    }
                }
            }
        };
    }

    private static AgentMessage BuildMarginPerLessonTypeReport()
    {
        var rows = new (string Type, decimal Price, decimal Cost)[]
        {
            ("Körlektion B, 60 min",  680m, 410m),
            ("Körlektion B, 90 min",  995m, 600m),
            ("Riskutbildning 1+2",   2_400m, 1_350m),
            ("Motorvägslektion",      850m, 560m),
            ("Intro / övrigt",        450m, 240m),
        };

        var summary = rows.Select(r => new
        {
            r.Type, r.Price, r.Cost,
            Margin = r.Price - r.Cost,
            MarginPct = (r.Price - r.Cost) / r.Price * 100m
        }).OrderByDescending(x => x.MarginPct).ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{summary.First().Type}** har högst marginal med **{summary.First().MarginPct:F0}%**. Riskutbildningen står för bäst kronmässig marginal per tillfälle.",
            Report = new AgentReport
            {
                Title = "Marginal per lektionstyp",
                Summary = "Pris, kostnad och bruttomarginal",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Marginal per lektionstyp (kr)",
                        Categories = summary.Select(x => x.Type).ToList(),
                        Series = { new ChartSeries { Name = "Marginal", Values = summary.Select(x => (double)x.Margin).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Lektionstyp", "Pris", "Kostnad", "Marginal", "Marginal %" },
                        Rows = summary.Select(x => new List<string>
                        {
                            x.Type,
                            $"{x.Price:N0} kr",
                            $"{x.Cost:N0} kr",
                            $"{x.Margin:N0} kr",
                            $"{x.MarginPct:F0}%",
                        }).ToList()
                    }
                }
            }
        };
    }

    private static AgentMessage BuildMarginPerTeacherReport(BookingService booking)
    {
        var teachers = booking.Teachers.Take(6).ToList();
        var revenue = new[] { 480_000.0, 440_000, 395_000, 360_000, 320_000, 280_000 };
        var costs   = new[] { 295_000.0, 280_000, 260_000, 245_000, 230_000, 215_000 };

        var rows = teachers.Select((t, i) =>
        {
            var rev = revenue[Math.Min(i, revenue.Length - 1)];
            var cost = costs[Math.Min(i, costs.Length - 1)];
            return new
            {
                Name = t.Name,
                Revenue = rev,
                Cost = cost,
                Margin = rev - cost,
                MarginPct = (rev - cost) / rev * 100.0,
            };
        }).OrderByDescending(x => x.Margin).ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{rows.First().Name}** har högst nettomarginal i år ({rows.First().Margin:N0} kr, {rows.First().MarginPct:F0}%). Snittmarginal: {rows.Average(r => r.MarginPct):F0}%.",
            Report = new AgentReport
            {
                Title = "Marginal per lärare",
                Summary = "Intäkt minus lönekostnad och rörlig fordonskostnad, ackumulerat i år",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Nettomarginal per lärare (kr)",
                        Categories = rows.Select(r => r.Name).ToList(),
                        Series = { new ChartSeries { Name = "Marginal", Values = rows.Select(r => r.Margin).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Lärare", "Intäkt", "Kostnad", "Marginal", "Marginal %" },
                        Rows = rows.Select(r => new List<string>
                        {
                            r.Name,
                            $"{r.Revenue:N0} kr",
                            $"{r.Cost:N0} kr",
                            $"{r.Margin:N0} kr",
                            $"{r.MarginPct:F0}%",
                        }).ToList()
                    }
                }
            }
        };
    }

    // ── Resource-rapporter ────────────────────────────────────────────────────

    /// <summary>Demo-beläggning per resurs (%) — deterministisk.</summary>
    private static double UtilizationFor(Resource r) => r.Type switch
    {
        ResourceType.Car        => 88 - (r.Id * 4 % 20),     // 68–88%
        ResourceType.Motorcycle => 58 - (r.Id * 3 % 18),     // 40–58% (säsong)
        ResourceType.Trailer    => 32 - (r.Id * 5 % 12),     // 20–32%
        ResourceType.Classroom  => 72 - (r.Id * 4 % 22),     // 50–72%
        ResourceType.Simulator  => 35,
        _                       => 50,
    };

    private static string TypeLabel(ResourceType t) => t switch
    {
        ResourceType.Car        => "Bil",
        ResourceType.Motorcycle => "Motorcykel",
        ResourceType.Trailer    => "Släp",
        ResourceType.Classroom  => "Sal",
        ResourceType.Simulator  => "Simulator",
        _                       => "Övrigt",
    };

    private static AgentMessage BuildResourceInventoryReport(BookingService booking)
    {
        var resources = booking.Resources;
        var available = resources.Count(r => r.IsAvailable);
        var inService = resources.Count(r => !r.IsAvailable);

        var byType = resources.GroupBy(r => r.Type)
            .Select(g => new { Type = TypeLabel(g.Key), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"Totalt **{resources.Count} resurser** ({available} tillgängliga, {inService} på service). Fördelning: {string.Join(", ", byType.Select(b => $"{b.Count} {b.Type.ToLower()}"))}.",
            Report = new AgentReport
            {
                Title = "Resursinventarie",
                Summary = "Alla fordon, salar och simulator",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Totalt antal", Value = $"{resources.Count}" },
                            new() { Label = "Tillgängliga", Value = $"{available}" },
                            new() { Label = "På service",   Value = $"{inService}", Trend = inService > 0 ? "bevaka" : "" },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Antal per typ",
                        Categories = byType.Select(b => b.Type).ToList(),
                        Series = { new ChartSeries { Name = "Antal", Values = byType.Select(b => (double)b.Count).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Heading = "Detaljer",
                        Columns = new() { "Resurs", "Typ", "Reg.nr / Plats", "År", "Mil/Kapacitet", "Status" },
                        Rows = resources.Select(r => new List<string>
                        {
                            r.Name,
                            TypeLabel(r.Type),
                            r.RegistrationNumber ?? (r.Type == ResourceType.Classroom ? "—" : "—"),
                            r.ModelYear?.ToString() ?? "—",
                            r.Capacity is int c ? $"{c} platser"
                                : r.Mileage is int m ? $"{m / 10:N0} mil"
                                : "—",
                            r.IsAvailable ? "I drift" : "På service",
                        }).ToList(),
                    }
                }
            }
        };
    }

    private static AgentMessage BuildResourceUtilizationReport(BookingService booking)
    {
        var rows = booking.Resources
            .Where(r => r.Type != ResourceType.Other)
            .Select(r => new { r.Name, Type = TypeLabel(r.Type), Util = UtilizationFor(r), r.IsAvailable })
            .OrderByDescending(x => x.Util)
            .ToList();

        var avg = rows.Average(x => x.Util);
        var top = rows.First();
        var bottom = rows.Last();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{top.Name}** har högst beläggning ({top.Util:F0}%). Snittet ligger på {avg:F0}%. Lägst belagd: **{bottom.Name}** ({bottom.Util:F0}%).",
            Report = new AgentReport
            {
                Title = "Beläggning per resurs",
                Summary = "Senaste 30 dagarna",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Beläggningsgrad (%)",
                        Categories = rows.Select(r => r.Name).ToList(),
                        Series = { new ChartSeries { Name = "Beläggning", Values = rows.Select(r => r.Util).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Resurs", "Typ", "Beläggning", "Status" },
                        Rows = rows.Select(r => new List<string>
                        {
                            r.Name,
                            r.Type,
                            $"{r.Util:F0}%",
                            r.IsAvailable
                                ? (r.Util >= 85 ? "Mycket hög" : r.Util >= 70 ? "Hög" : r.Util >= 50 ? "Medel" : "Låg")
                                : "På service",
                        }).ToList()
                    }
                }
            }
        };
    }

    private static AgentMessage BuildResourceUtilizationByTypeReport(BookingService booking)
    {
        var byType = booking.Resources
            .Where(r => r.Type != ResourceType.Other)
            .GroupBy(r => r.Type)
            .Select(g => new
            {
                Type = TypeLabel(g.Key),
                Count = g.Count(),
                AvgUtil = g.Average(UtilizationFor),
            })
            .OrderByDescending(x => x.AvgUtil)
            .ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = $"**{byType.First().Type}** har högst snittbeläggning ({byType.First().AvgUtil:F0}%). Släp ligger lågt — typiskt eftersom de bara används för BE-utbildning.",
            Report = new AgentReport
            {
                Title = "Beläggning per resurstyp",
                Summary = "Snitt senaste 30 dagarna, aggregerat per kategori",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Snittbeläggning per typ (%)",
                        Categories = byType.Select(b => b.Type).ToList(),
                        Series = { new ChartSeries { Name = "Beläggning", Values = byType.Select(b => b.AvgUtil).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Columns = new() { "Typ", "Antal", "Snittbeläggning" },
                        Rows = byType.Select(b => new List<string>
                        {
                            b.Type,
                            $"{b.Count}",
                            $"{b.AvgUtil:F0}%",
                        }).ToList()
                    }
                }
            }
        };
    }

    private static AgentMessage BuildResourceCostsReport(BookingService booking)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        // Estimera körda timmar senaste 30 dgr utifrån beläggning (8h arbetsdag * 22 dgr).
        const double availableHours = 8.0 * 22.0;

        var rows = booking.Resources
            .Where(r => r.Type is ResourceType.Car or ResourceType.Motorcycle or ResourceType.Trailer)
            .Select(r =>
            {
                var hours = availableHours * UtilizationFor(r) / 100.0;
                var cost  = (double)(r.HourlyCost ?? 0m) * hours;
                var daysToService = r.NextServiceDate.HasValue
                    ? (r.NextServiceDate.Value.DayNumber - today.DayNumber)
                    : (int?)null;
                return new
                {
                    r.Name,
                    Type = TypeLabel(r.Type),
                    Hours = hours,
                    Cost = cost,
                    NextService = r.NextServiceDate?.ToString("yyyy-MM-dd") ?? "—",
                    DaysToService = daysToService,
                };
            })
            .OrderByDescending(x => x.Cost)
            .ToList();

        var totalCost = rows.Sum(x => x.Cost);
        var soonService = rows.Where(x => x.DaysToService is int d && d <= 30).ToList();

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = soonService.Count > 0
                ? $"Total rörlig fordonskostnad senaste 30 dagarna: **{totalCost:N0} kr**. **{soonService.Count} fordon** har service inom 30 dagar."
                : $"Total rörlig fordonskostnad senaste 30 dagarna: **{totalCost:N0} kr**. Inga fordon har service inom 30 dagar.",
            Report = new AgentReport
            {
                Title = "Driftkostnad & service",
                Summary = "Rörliga kostnader senaste 30 dgr + kommande service",
                Blocks =
                {
                    new ReportBlock
                    {
                        Kind = BlockKind.KeyFigures,
                        Figures =
                        {
                            new() { Label = "Total driftkostnad", Value = $"{totalCost:N0} kr", Trend = "senaste 30 dgr" },
                            new() { Label = "Antal fordon",       Value = $"{rows.Count}" },
                            new() { Label = "Service inom 30 dgr", Value = $"{soonService.Count}", Trend = soonService.Count > 0 ? "boka in" : "OK" },
                        }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.BarChart,
                        Heading = "Driftkostnad per fordon (kr)",
                        Categories = rows.Select(r => r.Name).ToList(),
                        Series = { new ChartSeries { Name = "Kostnad", Values = rows.Select(r => r.Cost).ToList() } }
                    },
                    new ReportBlock
                    {
                        Kind = BlockKind.Table,
                        Heading = "Detaljer per fordon",
                        Columns = new() { "Fordon", "Typ", "Körda h (30 dgr)", "Kostnad", "Nästa service", "Dgr kvar" },
                        Rows = rows.Select(r => new List<string>
                        {
                            r.Name,
                            r.Type,
                            $"{r.Hours:F0} h",
                            $"{r.Cost:N0} kr",
                            r.NextService,
                            r.DaysToService?.ToString() ?? "—",
                        }).ToList()
                    }
                }
            }
        };
    }

    // ── Combine: slå ihop flera tool-resultat till en sammansatt rapport ──────

    /// <summary>
    /// Slår ihop flera <see cref="AgentMessage"/> till en. Används när
    /// LLM:n returnerar flera tool-calls eller TestLlm matchar flera intents
    /// i samma fråga ("visa både omsättning och beläggning för året").
    ///
    /// Varje deltools titel blir heading på dess första block, så det blir
    /// tydligt visuellt var en rapport slutar och nästa börjar.
    /// </summary>
    public static AgentMessage Combine(IReadOnlyList<AgentMessage> parts, string? overrideText = null)
    {
        if (parts.Count == 0)
            return new AgentMessage { Role = AgentRole.Agent, Text = "Inget svar." };

        if (parts.Count == 1)
        {
            return overrideText is null
                ? parts[0]
                : new AgentMessage { Role = AgentRole.Agent, Text = overrideText, Report = parts[0].Report };
        }

        var withReports = parts.Where(p => p.Report is not null).ToList();
        if (withReports.Count == 0)
        {
            return new AgentMessage
            {
                Role = AgentRole.Agent,
                Text = overrideText ?? string.Join("\n\n", parts.Select(p => p.Text)),
            };
        }

        var combinedBlocks = new List<ReportBlock>();
        foreach (var part in withReports)
        {
            var report = part.Report!;
            var firstBlockHeadingPrefix = $"▸ {report.Title}";

            for (int i = 0; i < report.Blocks.Count; i++)
            {
                var b = report.Blocks[i];
                // Klona blocket men lägg deltools-titeln som heading på första blocket
                // (övriga block behåller sin egen heading om den finns).
                combinedBlocks.Add(new ReportBlock
                {
                    Kind = b.Kind,
                    Heading = i == 0
                        ? (string.IsNullOrEmpty(b.Heading) ? firstBlockHeadingPrefix : $"{firstBlockHeadingPrefix} – {b.Heading}")
                        : b.Heading,
                    Figures = b.Figures,
                    Categories = b.Categories,
                    Series = b.Series,
                    Columns = b.Columns,
                    Rows = b.Rows,
                });
            }
        }

        return new AgentMessage
        {
            Role = AgentRole.Agent,
            Text = overrideText ?? string.Join(" ", parts.Select(p => p.Text)),
            Report = new AgentReport
            {
                Title = string.Join(" + ", withReports.Select(p => p.Report!.Title)),
                Summary = "Sammanslagen rapport från flera underlag",
                Blocks = combinedBlocks,
            }
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private const string EmptyObjectSchema = """{ "type": "object", "properties": {}, "required": [] }""";

    private static int GetInt(JsonElement args, string name, int fallback)
    {
        if (args.ValueKind == JsonValueKind.Object &&
            args.TryGetProperty(name, out var p) &&
            p.ValueKind == JsonValueKind.Number &&
            p.TryGetInt32(out var v))
            return v;
        return fallback;
    }

    private static string? GetString(JsonElement args, string name)
    {
        if (args.ValueKind == JsonValueKind.Object &&
            args.TryGetProperty(name, out var p) &&
            p.ValueKind == JsonValueKind.String)
            return p.GetString();
        return null;
    }

    private static List<string> Last6MonthLabels(DateTime from)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE");
        var list = new List<string>();
        for (int i = 5; i >= 0; i--)
            list.Add(from.AddMonths(-i).ToString("MMM", culture));
        return list;
    }

    private static List<string> Last12MonthLabels(DateTime from)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE");
        var list = new List<string>();
        for (int i = 11; i >= 0; i--)
            list.Add(from.AddMonths(-i).ToString("MMM", culture));
        return list;
    }
}
