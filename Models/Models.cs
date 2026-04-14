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

// ── Articles (from PIM) ───────────────────────────────────────────────────────

/// <summary>
/// Represents a sellable article from the PIM system.
/// In production this would be fetched from PIM; here it is mocked in-memory.
/// </summary>
public class Article
{
    public int    Id            { get; set; }
    /// <summary>Article number as shown in PIM / invoice, e.g. "KB001".</summary>
    public string ArticleNumber { get; set; } = "";
    public string Name          { get; set; } = "";
    /// <summary>Price in SEK excl. VAT.</summary>
    public decimal Price        { get; set; }
}

// ── Lesson types ──────────────────────────────────────────────────────────────

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
    /// <summary>When false, the block occupies time in the schedule but cannot be booked by students (e.g. lunch, break).</summary>
    public bool IsBookable { get; set; } = true;
    /// <summary>When true, the teacher must verify student ID before the lesson starts.</summary>
    public bool RequiresIdCheck { get; set; } = false;
    /// <summary>Linked PIM article. Null for non-sellable types (e.g. lunch).</summary>
    public int? ArticleId { get; set; }
}

// ── Schedule planning ─────────────────────────────────────────────────────────

/// <summary>
/// A single time block within a schedule template cycle.
/// </summary>
public class ScheduleTimeBlock
{
    public int Id { get; set; }
    /// <summary>Which week of the cycle this block belongs to (1–4).</summary>
    public int WeekNumber { get; set; } = 1;
    /// <summary>Day of week: 1 = Måndag … 7 = Söndag.</summary>
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public int LessonTypeId { get; set; }
}

/// <summary>
/// A reusable schedule template assigned to a teacher. Contains time blocks
/// repeated over a 1–4 week cycle.
/// </summary>
public class ScheduleTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    /// <summary>Length of repeating cycle: 1–4 weeks.</summary>
    public int CycleWeeks { get; set; } = 1;
    /// <summary>null = global mall (tillgänglig för alla lärare).</summary>
    public int? TeacherId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<ScheduleTimeBlock> TimeBlocks { get; set; } = new();
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
    public int    Id          { get; set; }
    public string Name        { get; set; } = "";
    /// <summary>Tilltalsnamn / preferred first name</summary>
    public string NickName    { get; set; } = "";
    public string Email       { get; set; } = "";
    /// <summary>Mobilnummer</summary>
    public string Phone       { get; set; } = "";
    public string Address     { get; set; } = "";
    public int    Age         { get; set; }
    /// <summary>"Man", "Kvinna" or "Annat"</summary>
    public string Gender      { get; set; } = "";
    public bool   SmsReminder { get; set; } = true;
    public DateTime CreatedAt   { get; set; }
    /// <summary>Source system, e.g. "Optimapro webb"</summary>
    public string CreatedFrom { get; set; } = "";
    /// <summary>URL to avatar/photo</summary>
    public string PhotoUrl    { get; set; } = "";
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
    /// <summary>Emergency/guardian contacts for the student.</summary>
    public List<ContactPerson> ContactPersons { get; set; } = new();
}

public class ContactPerson
{
    public string Name     { get; set; } = "";
    /// <summary>Relation to student, e.g. "Mamma", "Pappa", "Vårdnadshavare"</summary>
    public string Relation { get; set; } = "";
    public string Phone    { get; set; } = "";
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

    /// Checkboxes for lesson assessment.
    public bool Demonstrated { get; set; }
    public bool Instructed   { get; set; }
    public bool Independent  { get; set; }

    /// Free-text comment for the whole lesson.
    public string LessonComment { get; set; } = "";

    /// Education plan moments linked to this lesson booking.
    public List<LessonMoment> LinkedMoments { get; set; } = new();

    /// Set on template-generated slots: the original start time from the block.
    /// Used to identify which exception to create/update when moving or deleting.
    public TimeOnly? TemplateOriginalStart { get; set; }
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
    /// <summary>
    /// När true visas enbart fullbokade pass (individuella alltid fulla; grupplektioner
    /// visas bara om antal bokade elever == MaxStudents).
    /// </summary>
    public bool ShowOnlyFullyBooked { get; set; } = false;
    public List<int> SelectedTeacherIds { get; set; } = new();
    public List<int> SelectedResourceIds { get; set; } = new();
    public List<int> SelectedLessonTypeIds { get; set; } = new();
}

// ── Schedule exception (manual override of a template-generated slot) ─────────

/// <summary>
/// Records a manual edit to a single occurrence of a template slot.
/// IsRemoved = true  → slot is hidden this day.
/// NewStartTime != null → slot is moved to that time.
/// </summary>
public class ScheduleException
{
    public int      Id                { get; set; }
    public int      TeacherId         { get; set; }
    public DateOnly Date              { get; set; }
    public TimeOnly OriginalStartTime { get; set; }
    public int      LessonTypeId      { get; set; }
    public bool     IsRemoved         { get; set; }
    public TimeOnly? NewStartTime     { get; set; }
}

