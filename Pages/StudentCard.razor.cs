using System.Globalization;
using BookingDemo.Components;
using BookingDemo.Models;
using BookingDemo.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BookingDemo.Pages;

public partial class StudentCard
{
    [Inject] private BookingService BookingService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    [Parameter] public string StudentName { get; set; } = "";

    private StudentProfile? _profile;
    private string _activeCategory       = "";
    private string _activeTheoryCategory = "";

    // ── Elevinfo redigering ───────────────────────────────────────────────────
    private bool   _editMode        = false;
    private string _editNickName    = "";
    private string _editEmail       = "";
    private string _editPhone       = "";
    private string _editAddress     = "";
    private int    _editAge;
    private string _editGender      = "";
    private bool   _editSmsReminder = true;
    private List<ContactPerson> _editContacts = new();

    private void StartEdit()
    {
        _editNickName    = _profile!.NickName;
        _editEmail       = _profile.Email;
        _editPhone       = _profile.Phone;
        _editAddress     = _profile.Address;
        _editAge         = _profile.Age;
        _editGender      = _profile.Gender;
        _editSmsReminder = _profile.SmsReminder;
        _editContacts    = _profile.ContactPersons
                               .Select(c => new ContactPerson { Name=c.Name, Relation=c.Relation, Phone=c.Phone })
                               .ToList();
        _editMode = true;
    }

    private void SaveEdit()
    {
        _profile!.NickName    = _editNickName;
        _profile.Email        = _editEmail;
        _profile.Phone        = _editPhone;
        _profile.Address      = _editAddress;
        _profile.Age          = _editAge;
        _profile.Gender       = _editGender;
        _profile.SmsReminder  = _editSmsReminder;
        _profile.ContactPersons = _editContacts
                                      .Select(c => new ContactPerson { Name=c.Name, Relation=c.Relation, Phone=c.Phone })
                                      .ToList();
        _editMode = false;
    }

    private void CancelEdit() => _editMode = false;

    private void AddEditContact() =>
        _editContacts.Add(new ContactPerson { Name="", Relation="", Phone="" });

    // Which license categories have the theory-area detail panel expanded
    private HashSet<string> _theoryDetailsExpanded = new();

    // Which license categories have the praktik-area detail panel expanded
    private HashSet<string> _praktikDetailsExpanded = new();

    // Utbildning tab state
    private HashSet<int>    _expandedLessonIds = new();
    private HashSet<string> _openCommentIds    = new();
    private static readonly CultureInfo _sv = new("sv-SE");
    private bool _showAllFuture  = false;
    private bool _showAllPast    = false;
    private const int LessonPreviewCount = 3;

    /// <summary>True if the past event has no moments, or has a non-approved moment (Score &lt; 3) without a comment.</summary>
    private static bool NeedsAttention(CalendarEvent e) =>
        e.LinkedMoments.Count == 0 ||
        e.LinkedMoments.Any(m => m.Score < 3 && string.IsNullOrWhiteSpace(m.Comment));

    private void ToggleTheoryDetails(string cat)
    {
        if (!_theoryDetailsExpanded.Add(cat))
            _theoryDetailsExpanded.Remove(cat);
        StateHasChanged();
    }

    private void TogglePraktikDetails(string cat)
    {
        if (!_praktikDetailsExpanded.Add(cat))
            _praktikDetailsExpanded.Remove(cat);
        StateHasChanged();
    }

    /// <summary>
    /// Returns progress per EduPlan section, aggregated across all lessons.
    /// For each GlobalId, the highest score across all lessons is used.
    /// </summary>
    private List<(string Name, int Approved, int Ok, int InProgress, int NotStarted, int Total)>
        GetPraktikSectionProgress()
    {
        if (_profile?.EduPlan == null) return new();

        var studentName = _profile.Name;
        var allEvents   = BookingService.GetEventsForStudent(studentName).ToList();

        // For each GlobalId, keep the highest score across all lessons
        var bestScore = new Dictionary<string, int>();
        foreach (var evt in allEvents)
        {
            foreach (var m in evt.LinkedMoments)
            {
                if (!bestScore.TryGetValue(m.GlobalId, out var existing) || m.Score > existing)
                    bestScore[m.GlobalId] = m.Score;
            }
        }

        var result = new List<(string, int, int, int, int, int)>();
        foreach (var section in _profile.EduPlan.Sections)
        {
            int approved = 0, ok = 0, inProgress = 0, notStarted = 0;
            foreach (var item in section.Items)
            {
                var score = bestScore.TryGetValue(item.GlobalId, out var s) ? s : 0;
                if      (score >= 4) approved++;
                else if (score == 3) ok++;
                else if (score >= 1) inProgress++;
                else                 notStarted++;
            }
            if (section.Items.Count > 0)
                result.Add((section.Title, approved, ok, inProgress, notStarted, section.Items.Count));
        }
        return result;
    }

    /// <summary>Renders a small SVG donut chart for a section's moment progress.</summary>
    private static Microsoft.AspNetCore.Components.MarkupString PieChartSvg(
        int approved, int ok, int inProgress, int notStarted, int total)
    {
        if (total == 0) return (Microsoft.AspNetCore.Components.MarkupString)"";

        var segments = new[]
        {
            (approved,    "#4CAF50"),   // Godkänd
            (ok,          "#8BC34A"),   // Godtagbar
            (inProgress,  "#FF9800"),   // Påbörjad
            (notStarted,  "#E0E0E0"),   // Ej påbörjad
        };

        const double R  = 38;  // outer radius
        const double ri = 22;  // inner radius (donut hole)
        const double cx = 50, cy = 50;
        const double TWO_PI = Math.PI * 2;

        var sb = new System.Text.StringBuilder();
        sb.Append("<svg viewBox='0 0 100 100' width='72' height='72' style='display:block'>");

        double startAngle = -Math.PI / 2; // top

        foreach (var (count, color) in segments)
        {
            if (count == 0) continue;
            double sweep    = count / (double)total * TWO_PI;
            double endAngle = startAngle + sweep;
            int    largeArc = sweep > Math.PI ? 1 : 0;

            double ox1 = cx + R  * Math.Cos(startAngle);
            double oy1 = cy + R  * Math.Sin(startAngle);
            double ox2 = cx + R  * Math.Cos(endAngle);
            double oy2 = cy + R  * Math.Sin(endAngle);

            double ix1 = cx + ri * Math.Cos(startAngle);
            double iy1 = cy + ri * Math.Sin(startAngle);
            double ix2 = cx + ri * Math.Cos(endAngle);
            double iy2 = cy + ri * Math.Sin(endAngle);

            sb.Append(
                $"<path d='M {ox1:F1} {oy1:F1} " +
                $"A {R} {R} 0 {largeArc} 1 {ox2:F1} {oy2:F1} " +
                $"L {ix2:F1} {iy2:F1} " +
                $"A {ri} {ri} 0 {largeArc} 0 {ix1:F1} {iy1:F1} Z' " +
                $"fill='{color}'/>");

            startAngle = endAngle;
        }

        // Center percentage label
        int pct = (int)Math.Round(approved / (double)total * 100);
        sb.Append($"<text x='50' y='54' text-anchor='middle' font-size='13' font-weight='700' fill='#424242'>{pct}%</text>");
        sb.Append("</svg>");

        return (Microsoft.AspNetCore.Components.MarkupString)sb.ToString();
    }

    // Labels shown under each tick mark: position 0 = "–", positions 1-5 = score
    private static readonly string[] _sliderLabels = ["–", "1", "2", "3", "4", "5"];

    private List<TrainingStep> ActiveSteps =>
        _profile?.TrainingStepsByCategory.TryGetValue(_activeCategory, out var s) == true
            ? s : new();

    private List<TrainingStep> ActiveTheorySteps =>
        _profile?.TheoryStepsByCategory.TryGetValue(_activeTheoryCategory, out var s) == true
            ? s : new();

    protected override void OnParametersSet()
    {
        var decoded = Uri.UnescapeDataString(StudentName);
        _profile = BookingService.GetStudentProfile(decoded);
        var first = _profile.LicenseCategories.FirstOrDefault() ?? "";
        _activeCategory       = first;
        _activeTheoryCategory = first;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ScoreColor(int score) => score switch
    {
        0         => "#9e9e9e",
        1 or 2    => "#f44336",
        3         => "#ff9800",
        4         => "#8bc34a",
        5         => "#4caf50",
        _         => "#9e9e9e"
    };

    private static string ScoreLabel(int score) => score switch
    {
        0 => "Ej bedömd",
        1 => "1 – Ej godkänd",
        2 => "2 – Under förväntan",
        3 => "3 – Godtagbar",
        4 => "4 – Godkänd",
        5 => "5 – Mycket bra",
        _ => ""
    };

    private static Color SliderColor(int score) => score switch
    {
        0         => Color.Default,
        1 or 2    => Color.Error,
        3         => Color.Warning,
        4 or 5    => Color.Success,
        _         => Color.Default
    };

    /// <summary>Color for a theory area progress bar based on completion percentage.</summary>
    private static string AreaBarColor(int pct) => pct switch
    {
        < 50 => "#f44336",   // red
        < 75 => "#ff9800",   // orange
        < 90 => "#8bc34a",   // light green
        _    => "#4caf50",   // green
    };

    private static string StepBadgeStyle(TrainingStep s)
    {
        var bg = s.Score switch
        {
            0         => "#e0e0e0",
            1 or 2    => "#f44336",
            3         => "#ff9800",
            4 or 5    => "#4caf50",
            _         => "#e0e0e0"
        };
        return $"background-color:{bg};color:{(s.Score == 0 ? "#757575" : "white")};font-size:11px;width:20px;height:20px;min-width:20px";
    }

    // ── Utbildning tab helpers ─────────────────────────────────────────────────

    private void ToggleLessonExpand(int eventId)
    {
        if (!_expandedLessonIds.Add(eventId))
            _expandedLessonIds.Remove(eventId);
    }

    private void ToggleComment(string key)
    {
        if (!_openCommentIds.Add(key))
            _openCommentIds.Remove(key);
    }

    private bool IsCommentOpen(string key) => _openCommentIds.Contains(key);

    private static string MomentStatusColor(int score) => score switch
    {
        >= 3 => "#388E3C",
        > 0  => "#0097A7",
        _    => "#BDBDBD",
    };

    private static string MomentStatusLabel(int score) => score switch
    {
        0     => "Ej påbörjat",
        1 or 2 => "Påbörjat",
        _     => "Godkänd",
    };

    private static List<MomentGroup> EmptyMomentGroups() => new();

    private static List<LessonMoment> CastToMomentList(object data) => (List<LessonMoment>)data;

    /// <summary>Group a lesson's LinkedMoments by their parent EduPlanSection.</summary>
    private static List<MomentGroup> GroupMomentsBySection(
        CalendarEvent evt, EduPlan plan)
    {
        var result = new List<MomentGroup>();
        if (evt.LinkedMoments.Count == 0) return result;

        foreach (var section in plan.Sections)
        {
            var inSection = evt.LinkedMoments
                .Where(m => section.Items.Any(i => i.GlobalId == m.GlobalId))
                .ToList();
            if (inSection.Count > 0)
                result.Add(new MomentGroup(section, inSection));
        }
        return result;
    }

    private async Task OpenMomentDialog(CalendarEvent evt)
    {
        if (_profile == null || _profile.EduPlan == null) return;
        var plan = _profile.EduPlan;

        var parameters = new DialogParameters<LessonMomentDialog>
        {
            { x => x.Plan,            plan                 },
            { x => x.ExistingMoments, evt.LinkedMoments    },
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
        var dialog  = await DialogService.ShowAsync<LessonMomentDialog>("Utbildningsmoment", parameters, options);
        var result  = await dialog.Result;

        if (result is { Canceled: false } && result.Data is not null)
        {
            evt.LinkedMoments = CastToMomentList(result.Data);
            StateHasChanged();
        }
    }
}
