using MudBlazor;

namespace BookingDemo.Models;

// ── Teachers ─────────────────────────────────────────────────────────────────

public class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Initials { get; set; } = "";
    /// <summary>Primary hex color (solid border / chip)</summary>
    public string Color { get; set; } = "";
    /// <summary>Light background hex color for event blocks</summary>
    public string LightColor { get; set; } = "";
    public bool IsSelected { get; set; } = true;
    /// <summary>Teacher group: 1 or 2.</summary>
    public int Group { get; set; } = 1;
}

// ── Lesson types (1–7) ────────────────────────────────────────────────────────

public class LessonType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "directions_car";
    public int DefaultDurationMinutes { get; set; } = 60;
    /// <summary>Max number of students per session. 1 = individual lesson. &gt;1 = group (e.g. Riskutbildning = 20).</summary>
    public int MaxStudents { get; set; } = 1;
    /// <summary>Solid accent color (border, badge).</summary>
    public string Color { get; set; } = "#1565C0";
    /// <summary>Light background color for event blocks.</summary>
    public string LightColor { get; set; } = "#E3F2FD";
}

// ── Resources ─────────────────────────────────────────────────────────────────

public enum ResourceType { Car, Classroom, Simulator, Other }

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ResourceType Type { get; set; }
    public bool IsAvailable { get; set; } = true;

    public string Icon => Type switch
    {
        ResourceType.Car       => MudBlazor.Icons.Material.Filled.DirectionsCar,
        ResourceType.Classroom => MudBlazor.Icons.Material.Filled.MeetingRoom,
        ResourceType.Simulator => MudBlazor.Icons.Material.Filled.SportsEsports,
        _                      => MudBlazor.Icons.Material.Filled.Category,
    };
}

// ── Students ──────────────────────────────────────────────────────────────────

public class Student
{
    public int    Id   { get; set; }
    public string Name { get; set; } = "";
}

// ── Student profile (extended, for Elevkort) ─────────────────────────────────

public class TrainingStep
{
    public int    Number  { get; set; }
    public string Name    { get; set; } = "";
    /// <summary>0 = not assessed, 1–5 = score</summary>
    public int    Score   { get; set; } = 0;
    public string Comment { get; set; } = "";
}

public class StudentProfile
{
    public int    Id         { get; set; }
    public string Name       { get; set; } = "";
    public string Email      { get; set; } = "";
    public string Phone      { get; set; } = "";
    /// <summary>URL to avatar/photo</summary>
    public string PhotoUrl   { get; set; } = "";
    /// <summary>All active license categories, e.g. ["B", "AM"]</summary>
    public List<string> LicenseCategories { get; set; } = new();
    /// <summary>Practical driving steps keyed by license category.</summary>
    public Dictionary<string, List<TrainingStep>> TrainingStepsByCategory { get; set; } = new();
    /// <summary>Theory/knowledge steps keyed by license category.</summary>
    public Dictionary<string, List<TrainingStep>> TheoryStepsByCategory { get; set; } = new();
    /// <summary>Education plan (from JSON structure). Null if not applicable.</summary>
    public EduPlan? EduPlan { get; set; }
    /// <summary>Detailed theory area progress (26 areas, 0–100 %).</summary>
    public List<TheoryAreaProgress> TheoryAreaProgress { get; set; } = new();
}

// ── Education plan (from JSON structure) ─────────────────────────────────────

public class EduPlanItem
{
    public string GlobalId { get; set; } = "";
    public string Title    { get; set; } = "";
    /// <summary>Sub-mark, e.g. "ab", "abcd".</summary>
    public string Mark     { get; set; } = "";
    public bool   Done     { get; set; } = false;
}

public class EduPlanSection
{
    public string Title { get; set; } = "";
    /// <summary>Section label, e.g. "Mom 1". Empty for non-numbered sections.</summary>
    public string Mark  { get; set; } = "";
    public List<EduPlanItem> Items { get; set; } = new();
}

public class EduPlan
{
    public string Title { get; set; } = "";
    public List<EduPlanSection> Sections { get; set; } = new();
    public int TotalItems => Sections.Sum(s => s.Items.Count);
    public int DoneItems  => Sections.Sum(s => s.Items.Count(i => i.Done));
}

// ── Theory area progress ──────────────────────────────────────────────────────

public class TheoryAreaProgress
{
    public string Name       { get; set; } = "";
    /// <summary>0–100 percent completed.</summary>
    public int    Percentage { get; set; }
}

// ── Pickup locations ──────────────────────────────────────────────────────────

public class PickupLocation
{
    public int    Id      { get; set; }
    public string Name    { get; set; } = "";
    public string Address { get; set; } = "";
}

// ── Calendar events ───────────────────────────────────────────────────────────

/// <summary>
/// A single block on the calendar.
/// IsBooked = false  → available slot (teacher has offered this time)
/// IsBooked = true   → confirmed booking by a student
/// </summary>
public class CalendarEvent
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    public string StudentName { get; set; } = "";
    public int? LessonTypeId { get; set; }
    public List<int> ResourceIds { get; set; } = new();
    public string Notes { get; set; } = "";
    public bool IsRecurring { get; set; }
    public int? PickupLocationId { get; set; }

    /// Extra students beyond the first (used for "Delad körlektion").
    public List<string> ExtraStudents { get; set; } = new();

    /// All student names: primary + any extras.
    public IEnumerable<string> AllStudentNames =>
        string.IsNullOrWhiteSpace(StudentName)
            ? ExtraStudents
            : ExtraStudents.Prepend(StudentName);

    public TimeSpan Duration => EndTime - StartTime;
    public double DurationHours => Duration.TotalHours;

    /// Education plan moments linked to this lesson booking.
    public List<LessonMoment> LinkedMoments { get; set; } = new();
}

// ── Lesson moment (education plan item linked to a booking) ───────────────────

public class LessonMoment
{
    /// <summary>GlobalId of the EduPlanItem this moment references.</summary>
    public string GlobalId { get; set; } = "";
    /// <summary>Denormalized title for display without looking up the plan.</summary>
    public string Title    { get; set; } = "";
    /// <summary>Score 0–5. 0 = not started, 1–2 = in progress, 3–5 = approved.</summary>
    public int    Score    { get; set; } = 0;
    public string Comment  { get; set; } = "";

    public string StatusLabel => Score switch
    {
        0     => "Ej påbörjat",
        1 or 2 => "Påbörjat",
        _     => "Godkänd",
    };
}

// ── Booking request (used in dialog) ─────────────────────────────────────────

public class BookingRequest
{
    public int TeacherId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string StudentName { get; set; } = "";
    public int LessonTypeId { get; set; } = 1;
    public List<string> ExtraStudents { get; set; } = new();
    public List<int> SelectedResourceIds { get; set; } = new();
    public string Notes { get; set; } = "";
    public int? PickupLocationId { get; set; }
}

// ── Filter state ──────────────────────────────────────────────────────────────

public class CalendarFilter
{
    public bool ShowOnlyAvailable { get; set; } = false;
    public bool ShowBooked { get; set; } = true;
    public bool ShowAvailable { get; set; } = true;
    public bool HideWeekends { get; set; } = false;
    public bool HideTeacherAvatars { get; set; } = false;
    public List<int> SelectedTeacherIds { get; set; } = new();
    public List<int> SelectedResourceIds { get; set; } = new();
    public List<int> SelectedLessonTypeIds { get; set; } = new();
}

