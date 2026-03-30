using BookingDemo.Models;
using MudBlazor;

namespace BookingDemo.Services;

/// <summary>
/// In-memory booking service with realistic mock data for a driving school.
/// </summary>
public class BookingService
{
    // ── Seed data ─────────────────────────────────────────────────────────────

    public List<Teacher> Teachers { get; } = new()
    {
        // ── Grupp 1 ──────────────────────────────────────────────────────────
        new() { Id = 1, Name = "Anna Lindgren",    Initials = "AL", Color = "#1565C0", LightColor = "#BBDEFB", IsSelected = true,  Group = 1 },
        new() { Id = 2, Name = "Erik Svensson",    Initials = "ES", Color = "#00695C", LightColor = "#B2DFDB", IsSelected = true,  Group = 1 },
        new() { Id = 3, Name = "Maria Johansson",  Initials = "MJ", Color = "#6A1B9A", LightColor = "#E1BEE7", IsSelected = true,  Group = 1 },
        new() { Id = 4, Name = "Johan Persson",    Initials = "JP", Color = "#E65100", LightColor = "#FFE0B2", IsSelected = false, Group = 1 },
        new() { Id = 5, Name = "Petra Nilsson",    Initials = "PN", Color = "#558B2F", LightColor = "#DCEDC8", IsSelected = false, Group = 1 },
        new() { Id = 6, Name = "Lars Karlsson",    Initials = "LK", Color = "#880E4F", LightColor = "#FCE4EC", IsSelected = false, Group = 1 },
        // ── Grupp 2 ──────────────────────────────────────────────────────────
        new() { Id = 7, Name = "Sara Lindström",   Initials = "SL", Color = "#0277BD", LightColor = "#B3E5FC", IsSelected = false, Group = 2 },
        new() { Id = 8, Name = "Mikael Berggren",  Initials = "MB", Color = "#C62828", LightColor = "#FFCDD2", IsSelected = false, Group = 2 },
        new() { Id = 9, Name = "Lena Öberg",       Initials = "LÖ", Color = "#37474F", LightColor = "#CFD8DC", IsSelected = false, Group = 2 },
        new() { Id =10, Name = "Anders Holm",      Initials = "AH", Color = "#00838F", LightColor = "#E0F7FA", IsSelected = false, Group = 2 },
        new() { Id =11, Name = "Kristin Fransson", Initials = "KF", Color = "#6D4C41", LightColor = "#EFEBE9", IsSelected = false, Group = 2 },
    };

    public List<LessonType> LessonTypes { get; } = new()
    {
        new() { Id = 1, Name = "Introduktionslektion", Description = "Första lektionen – introduktion till körning", Icon = Icons.Material.Filled.EmojiTransportation, DefaultDurationMinutes = 50,  Color = "#1565C0", LightColor = "#BBDEFB" },
        new() { Id = 2, Name = "Körlektion B",         Description = "Standard körlektion för B-körkort",           Icon = Icons.Material.Filled.DirectionsCar,       DefaultDurationMinutes = 60,  Color = "#2E7D32", LightColor = "#C8E6C9" },
        new() { Id = 3, Name = "Motorvägslektion",     Description = "Körning på motorväg",                          Icon = Icons.Material.Filled.Speed,               DefaultDurationMinutes = 90,  Color = "#E65100", LightColor = "#FFE0B2" },
        new() { Id = 4, Name = "Körlektion kväll",     Description = "Mörkerövning",                                 Icon = Icons.Material.Filled.NightsStay,          DefaultDurationMinutes = 60,  Color = "#4527A0", LightColor = "#D1C4E9" },
        new() { Id = 5, Name = "Riskutbildning 1",     Description = "Halkbana och riskmedvetenhet",                 Icon = Icons.Material.Filled.Warning,             DefaultDurationMinutes = 120, Color = "#B71C1C", LightColor = "#FFCDD2", MaxStudents = 20 },
        new() { Id = 6, Name = "Riskutbildning 2",     Description = "Alkohol, droger & trafiksäkerhet",             Icon = Icons.Material.Filled.LocalBar,            DefaultDurationMinutes = 90,  Color = "#BF360C", LightColor = "#FFCCBC", MaxStudents = 20 },
        new() { Id = 7, Name = "Uppkörning",           Description = "Körprov hos Trafikverket",                     Icon = Icons.Material.Filled.FactCheck,           DefaultDurationMinutes = 45,  Color = "#00695C", LightColor = "#B2DFDB" },
        new() { Id = 8, Name = "Delad körlektion",     Description = "Flera elever delar på en lektion",              Icon = Icons.Material.Filled.Group,               DefaultDurationMinutes = 60,  Color = "#F57F17", LightColor = "#FFF9C4" },
        new() { Id = 9, Name = "Lunch",                Description = "Lunchpaus",                                     Icon = Icons.Material.Filled.Restaurant,           DefaultDurationMinutes = 60,  Color = "#78909C", LightColor = "#ECEFF1", IsBookable = false },
    };

    public List<Student> Students { get; } = new()
    {
        new() { Id =  1, Name = "Alice Bergström"    },
        new() { Id =  2, Name = "Björn Lindqvist"    },
        new() { Id =  3, Name = "Cecilia Holm"       },
        new() { Id =  4, Name = "David Ekström"      },
        new() { Id =  5, Name = "Emma Sjöberg"       },
        new() { Id =  6, Name = "Filip Gustafsson"   },
        new() { Id =  7, Name = "Gabriella Nordin"   },
        new() { Id =  8, Name = "Henrik Sandberg"    },
        new() { Id =  9, Name = "Ida Wallin"         },
        new() { Id = 10, Name = "Jakob Lund"         },
        new() { Id = 11, Name = "Karin Magnusson"    },
        new() { Id = 12, Name = "Lucas Eriksson"     },
        new() { Id = 13, Name = "Maja Petersson"     },
        new() { Id = 14, Name = "Nils Hansson"       },
        new() { Id = 15, Name = "Olivia Larsson"     },
        new() { Id = 16, Name = "Pontus Andersson"   },
        new() { Id = 17, Name = "Rebecka Johansson"  },
        new() { Id = 18, Name = "Samuel Nilsson"     },
        new() { Id = 19, Name = "Tilda Svensson"     },
        new() { Id = 20, Name = "Ulrika Karlsson"    },
        new() { Id = 21, Name = "Viktor Persson"     },
        new() { Id = 22, Name = "Wilma Olsson"       },
        new() { Id = 23, Name = "Axel Martinsson"    },
        new() { Id = 24, Name = "Bella Claesson"     },
        new() { Id = 25, Name = "Carl Hedström"      },
        new() { Id = 26, Name = "Diana Forsberg"     },
        new() { Id = 27, Name = "Elias Sundqvist"    },
        new() { Id = 28, Name = "Frida Björk"        },
        new() { Id = 29, Name = "Gustav Åberg"       },
        new() { Id = 30, Name = "Hannah Lindgren"    },
    };

    public IEnumerable<string> SearchStudents(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Students.Select(s => s.Name);
        return Students
            .Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Name);
    }

    // ── Student profiles (Elevkort) ────────────────────────────────────────────

    private static List<TrainingStep> MakeSteps() => new()
    {
        new() { Number =  1, Name = "Introduktion och trafikkunskap"       },
        new() { Number =  2, Name = "Fordonshantering – start och stopp"   },
        new() { Number =  3, Name = "Kurvkörning och styrning"             },
        new() { Number =  4, Name = "Backning och vändning"                },
        new() { Number =  5, Name = "Körning i korsningar"                 },
        new() { Number =  6, Name = "Filbyten och omkörning"               },
        new() { Number =  7, Name = "Körning i stadsmiljö"                 },
        new() { Number =  8, Name = "Körning på landsvägsfart"             },
        new() { Number =  9, Name = "Motorvägskörning"                     },
        new() { Number = 10, Name = "Körning i mörker och dåligt väder"    },
        new() { Number = 11, Name = "Eco-driving och bränsleekonomi"       },
        new() { Number = 12, Name = "Nödstopp och riskundvikande"          },
        new() { Number = 13, Name = "Parkering och manövrering"            },
        new() { Number = 14, Name = "Självständig körning"                 },
        new() { Number = 15, Name = "Avslutande prov och utvärdering"      },
    };

    private static List<TrainingStep> MakeTheorySteps() => new()
    {
        new() { Number =  1, Name = "Trafikregler och lagar"                    },
        new() { Number =  2, Name = "Vägmärken och vägmarkeringar"             },
        new() { Number =  3, Name = "Fordonskännedom och underhåll"            },
        new() { Number =  4, Name = "Väjningsregler i korsningar"              },
        new() { Number =  5, Name = "Hastighetsregler och säkerhetsavstånd"    },
        new() { Number =  6, Name = "Alkohol, droger och trötthet i trafiken"  },
        new() { Number =  7, Name = "Föraransvar och straffansvar"             },
        new() { Number =  8, Name = "Miljö och eco-driving"                    },
        new() { Number =  9, Name = "Motorvägs- och landsvägsregler"           },
        new() { Number = 10, Name = "Mörker-, regn- och vinterkörning"         },
        new() { Number = 11, Name = "Olyckor, första hjälpen och nödnummer"    },
        new() { Number = 12, Name = "Riskbedömning och samspel i trafiken"     },
    };

    private static Dictionary<string, List<TrainingStep>> TheoryCat(
        params (string cat, int assessed, string comment, int commentStep)[] entries)
    {
        var dict = new Dictionary<string, List<TrainingStep>>();
        var pattern = new[] { 4, 5, 5, 4, 5, 3, 4, 5, 5, 4, 5, 4 };
        foreach (var (cat, assessed, comment, commentStep) in entries)
        {
            dict[cat] = MakeTheorySteps().Select((s, i) => new TrainingStep
            {
                Number  = s.Number,
                Name    = s.Name,
                Score   = i < assessed ? pattern[i % pattern.Length] : 0,
                Comment = (i == commentStep && assessed > i) ? comment : ""
            }).ToList();
        }
        return dict;
    }
    private static Dictionary<string, List<TrainingStep>> TheoryCat(
        params (string cat, int assessed)[] entries)
    {
        var dict = new Dictionary<string, List<TrainingStep>>();
        var pattern = new[] { 4, 5, 5, 4, 5, 3, 4, 5, 5, 4, 5, 4 };
        foreach (var (cat, assessed) in entries)
        {
            dict[cat] = MakeTheorySteps().Select((s, i) => new TrainingStep
            {
                Number = s.Number, Name = s.Name,
                Score  = i < assessed ? pattern[i % pattern.Length] : 0,
                Comment = ""
            }).ToList();
        }
        return dict;
    }

    // Shared mutable store so progress edits persist in the session
    private readonly Dictionary<int, StudentProfile> _studentProfiles;

    // Named profiles for contacts/relatives who aren't in the main student list
    private static readonly Dictionary<string, StudentProfile> _namedProfiles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Maria Bergström"] = new()
            {
                Id = 9001, Name = "Maria Bergström", NickName = "Maria", Gender = "Kvinna", Age = 48,
                Email = "maria.bergstrom@example.com", Phone = "070-234 56 78",
                Address = "Rådmansgatan 14, 113 57 Stockholm",
                SmsReminder = false,
                CreatedAt   = new DateTime(2024, 3, 15), CreatedFrom = "Manuellt",
                PhotoUrl    = "https://randomuser.me/api/portraits/women/55.jpg",
                LicenseCategories         = [],
                TrainingStepsByCategory   = new(),
                TheoryStepsByCategory     = new(),
                ContactPersons = new() { new() { Name = "Alice Bergström", Relation = "Dotter", Phone = "070-123 45 67" } }
            },
            ["Lars Bergström"] = new()
            {
                Id = 9002, Name = "Lars Bergström", NickName = "Lars", Gender = "Man", Age = 51,
                Email = "lars.bergstrom@example.com", Phone = "073-456 78 90",
                Address = "Rådmansgatan 14, 113 57 Stockholm",
                SmsReminder = false,
                CreatedAt   = new DateTime(2024, 3, 15), CreatedFrom = "Manuellt",
                PhotoUrl    = "https://randomuser.me/api/portraits/men/60.jpg",
                LicenseCategories         = [],
                TrainingStepsByCategory   = new(),
                TheoryStepsByCategory     = new(),
                ContactPersons = new() { new() { Name = "Alice Bergström", Relation = "Dotter", Phone = "070-123 45 67" } }
            },
        };

    /// <summary>Build steps where the first <paramref name="assessed"/> have scores,
    /// using a repeating 3-4-5-4-5 pattern, with optional comment on step index
    /// <paramref name="commentStep"/>.</summary>
    private static List<TrainingStep> Steps(int assessed, string comment = "", int commentStep = -1)
    {
        var pattern = new[] { 3, 4, 5, 4, 5, 4, 3, 5, 4, 5, 5, 4, 4, 5, 5 };
        return MakeSteps().Select((s, i) => new TrainingStep
        {
            Number  = s.Number,
            Name    = s.Name,
            Score   = i < assessed ? pattern[i % pattern.Length] : 0,
            Comment = (i == commentStep && assessed > i) ? comment : ""
        }).ToList();
    }

    /// <summary>Shorthand to build the per-category steps dictionary.</summary>
    private static Dictionary<string, List<TrainingStep>> Cat(
        params (string cat, int assessed, string comment, int commentStep)[] entries)
    {
        var dict = new Dictionary<string, List<TrainingStep>>();
        foreach (var (cat, assessed, comment, commentStep) in entries)
            dict[cat] = Steps(assessed, comment, commentStep);
        return dict;
    }
    private static Dictionary<string, List<TrainingStep>> Cat(
        params (string cat, int assessed)[] entries)
    {
        var dict = new Dictionary<string, List<TrainingStep>>();
        foreach (var (cat, assessed) in entries)
            dict[cat] = Steps(assessed);
        return dict;
    }

    private static Dictionary<int, StudentProfile> BuildProfiles() => new()
    {
        // ── Id 1-10 ───────────────────────────────────────────────────────────
        [1] = new() { Id=1,  Name="Alice Bergström",   NickName="Alice",   Gender="Kvinna", Age=19,
            Email="alice.bergstrom@example.com",   Phone="070-123 45 67",
            Address="Rådmansgatan 14, 113 57 Stockholm",
            SmsReminder=true,  CreatedAt=new DateTime(2024,3,15),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/44.jpg",
            LicenseCategories=["B","AM"],
            TrainingStepsByCategory=Cat(
                ("B",  8, "Bra framsteg i kurvor men rattgreppet kan förbättras.", 2),
                ("AM", 3, "", -1)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  10, "Svårt med väjningsregler i komplexa korsningar.", 3),
                ("AM", 5,  "", -1)) },

        [2] = new() { Id=2,  Name="Björn Lindqvist",   NickName="Björn",   Gender="Man",    Age=22,
            Email="bjorn.lindqvist@example.com",   Phone="073-987 65 43",
            Address="Kungsgatan 5, 411 19 Göteborg",
            SmsReminder=true,  CreatedAt=new DateTime(2024,1,20),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/32.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 3)),
            TheoryStepsByCategory=TheoryCat(("B", 4)) },

        [3] = new() { Id=3,  Name="Cecilia Holm",      NickName="Ceci",    Gender="Kvinna", Age=25,
            Email="cecilia.holm@example.com",      Phone="070-234 56 78",
            Address="Drottninggatan 8, 211 25 Malmö",
            SmsReminder=false, CreatedAt=new DateTime(2023,11,10), CreatedFrom="Manuellt",
            PhotoUrl="https://randomuser.me/api/portraits/women/21.jpg",
            LicenseCategories=["B","BE"],
            TrainingStepsByCategory=Cat(
                ("B",  15, "Godkänd på alla moment. Redo för uppkörning.", 14),
                ("BE", 5,  "", -1)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  12, "Samtliga teoristeg avklarade med högt resultat.", 11),
                ("BE", 6,  "", -1)) },

        [4] = new() { Id=4,  Name="David Ekström",     NickName="David",   Gender="Man",    Age=18,
            Email="david.ekstrom@example.com",     Phone="076-345 67 89",
            Address="Vasagatan 22, 753 23 Uppsala",
            SmsReminder=true,  CreatedAt=new DateTime(2024,5,5),   CreatedFrom="Optimapro app",
            PhotoUrl="https://randomuser.me/api/portraits/men/17.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 11, "Svårigheter med filbyten på motorväg.", 5)),
            TheoryStepsByCategory=TheoryCat(
                ("B", 9, "Alkohol och droger-avsnittet kräver repetition.", 5)) },

        [5] = new() { Id=5,  Name="Emma Sjöberg",      NickName="Emma",    Gender="Kvinna", Age=17,
            Email="emma.sjoberg@example.com",      Phone="076-555 44 33",
            Address="Linnégatan 3, 582 24 Linköping",
            SmsReminder=true,  CreatedAt=new DateTime(2024,2,28),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/68.jpg",
            LicenseCategories=["B","A1"],
            TrainingStepsByCategory=Cat(
                ("B",  10, "Svårt att hålla rätt hastighet i mörker.", 9),
                ("A1", 2,  "", -1)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  8,  "", -1),
                ("A1", 1,  "", -1)) },

        [6] = new() { Id=6,  Name="Filip Gustafsson",  NickName="Filip",   Gender="Man",    Age=20,
            Email="filip.gustafsson@example.com",  Phone="070-456 78 90",
            Address="Strandvägen 45, 702 22 Örebro",
            SmsReminder=true,  CreatedAt=new DateTime(2024,4,10),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/55.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 6)),
            TheoryStepsByCategory=TheoryCat(("B", 7)) },

        [7] = new() { Id=7,  Name="Gabriella Nordin",  NickName="Gabby",   Gender="Kvinna", Age=16,
            Email="gabriella.nordin@example.com",  Phone="072-111 22 33",
            Address="Kyrkogatan 7, 602 24 Norrköping",
            SmsReminder=true,  CreatedAt=new DateTime(2024,6,1),   CreatedFrom="Optimapro app",
            PhotoUrl="https://randomuser.me/api/portraits/women/12.jpg",
            LicenseCategories=["AM"],
            TrainingStepsByCategory=Cat(("AM", 0)),
            TheoryStepsByCategory=TheoryCat(("AM", 2)) },

        [8] = new() { Id=8,  Name="Henrik Sandberg",   NickName="Henke",   Gender="Man",    Age=28,
            Email="henrik.sandberg@example.com",   Phone="073-567 89 01",
            Address="Järnvägsgatan 11, 252 24 Helsingborg",
            SmsReminder=false, CreatedAt=new DateTime(2023,9,15),  CreatedFrom="Importerad",
            PhotoUrl="https://randomuser.me/api/portraits/men/43.jpg",
            LicenseCategories=["B","BE"],
            TrainingStepsByCategory=Cat(
                ("B",  13, "Mycket bra vid nödstopp.", 11),
                ("BE", 7,  "Backning med släp kräver mer övning.", 3)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  11, "", -1),
                ("BE", 4,  "", -1)) },

        [9] = new() { Id=9,  Name="Ida Wallin",        NickName="Ida",     Gender="Kvinna", Age=21,
            Email="ida.wallin@example.com",        Phone="070-678 90 12",
            Address="Parkgatan 6, 554 24 Jönköping",
            SmsReminder=true,  CreatedAt=new DateTime(2024,3,20),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/35.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 7, "Osäker vid backning – behöver mer träning.", 3)),
            TheoryStepsByCategory=TheoryCat(("B", 6)) },

        [10] = new() { Id=10, Name="Jakob Lund",       NickName="Jakob",   Gender="Man",    Age=18,
            Email="jakob.lund@example.com",        Phone="076-789 01 23",
            Address="Torggatan 19, 903 26 Umeå",
            SmsReminder=true,  CreatedAt=new DateTime(2024,7,8),   CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/10.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 4)),
            TheoryStepsByCategory=TheoryCat(("B", 3)) },

        // ── Id 11-20 ──────────────────────────────────────────────────────────
        [11] = new() { Id=11, Name="Karin Magnusson",  NickName="Karin",   Gender="Kvinna", Age=24,
            Email="karin.magnusson@example.com",   Phone="070-890 12 34",
            Address="Bergsgatan 33, 503 38 Borås",
            SmsReminder=false, CreatedAt=new DateTime(2023,12,1),  CreatedFrom="Manuellt",
            PhotoUrl="https://randomuser.me/api/portraits/women/57.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 12, "Eco-driving är ett starkt område.", 10)),
            TheoryStepsByCategory=TheoryCat(
                ("B", 12, "Utmärkt på miljöavsnittet.", 7)) },

        [12] = new() { Id=12, Name="Lucas Eriksson",   NickName="Lucas",   Gender="Man",    Age=19,
            Email="lucas.eriksson@example.com",    Phone="073-901 23 45",
            Address="Storgatan 88, 852 30 Sundsvall",
            SmsReminder=true,  CreatedAt=new DateTime(2024,2,14),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/28.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 5)),
            TheoryStepsByCategory=TheoryCat(("B", 5)) },

        [13] = new() { Id=13, Name="Maja Petersson",   NickName="Maja",    Gender="Kvinna", Age=17,
            Email="maja.petersson@example.com",    Phone="070-012 34 56",
            Address="Alégatan 2, 222 24 Lund",
            SmsReminder=true,  CreatedAt=new DateTime(2024,5,22),  CreatedFrom="Optimapro app",
            PhotoUrl="https://randomuser.me/api/portraits/women/80.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 9, "Bra kontroll i stadsmiljö.", 6)),
            TheoryStepsByCategory=TheoryCat(("B", 10)) },

        [14] = new() { Id=14, Name="Nils Hansson",     NickName="Nils",    Gender="Man",    Age=23,
            Email="nils.hansson@example.com",      Phone="076-123 45 67",
            Address="Fredsgatan 15, 803 11 Gävle",
            SmsReminder=true,  CreatedAt=new DateTime(2024,1,9),   CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/64.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 2)),
            TheoryStepsByCategory=TheoryCat(("B", 2)) },

        [15] = new() { Id=15, Name="Olivia Larsson",   NickName="Olivia",  Gender="Kvinna", Age=20,
            Email="olivia.larsson@example.com",    Phone="070-234 56 79",
            Address="Kungsgatan 77, 632 20 Eskilstuna",
            SmsReminder=true,  CreatedAt=new DateTime(2023,10,30), CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/47.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 14, "Självständig körning är en styrka.", 13)),
            TheoryStepsByCategory=TheoryCat(
                ("B", 12, "Fullständig teorigenomgång klar.", 11)) },

        [16] = new() { Id=16, Name="Pontus Andersson", NickName="Pontus",  Gender="Man",    Age=22,
            Email="pontus.andersson@example.com",  Phone="073-345 67 90",
            Address="Fabriksgatan 4, 302 45 Halmstad",
            SmsReminder=false, CreatedAt=new DateTime(2024,4,17),  CreatedFrom="Importerad",
            PhotoUrl="https://randomuser.me/api/portraits/men/38.jpg",
            LicenseCategories=["B","AM","A1"],
            TrainingStepsByCategory=Cat(
                ("B",  7,  "", -1),
                ("AM", 15, "AM avklarad med bra resultat.", 14),
                ("A1", 4,  "Behöver öva mer på kopplingen.", 2)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  8,  "", -1),
                ("AM", 12, "AM-teori helt genomförd.", 11),
                ("A1", 3,  "", -1)) },

        [17] = new() { Id=17, Name="Rebecka Johansson",NickName="Becca",   Gender="Kvinna", Age=17,
            Email="rebecka.johansson@example.com", Phone="070-456 78 91",
            Address="Vasavägen 9, 392 33 Kalmar",
            SmsReminder=true,  CreatedAt=new DateTime(2024,6,10),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/25.jpg",
            LicenseCategories=["A1"],
            TrainingStepsByCategory=Cat(("A1", 1)),
            TheoryStepsByCategory=TheoryCat(("A1", 2)) },

        [18] = new() { Id=18, Name="Samuel Nilsson",   NickName="Sam",     Gender="Man",    Age=26,
            Email="samuel.nilsson@example.com",    Phone="076-567 89 02",
            Address="Slottsgatan 23, 652 25 Karlstad",
            SmsReminder=true,  CreatedAt=new DateTime(2023,8,25),  CreatedFrom="Manuellt",
            PhotoUrl="https://randomuser.me/api/portraits/men/72.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 10)),
            TheoryStepsByCategory=TheoryCat(("B", 9)) },

        [19] = new() { Id=19, Name="Tilda Svensson",   NickName="Tilda",   Gender="Kvinna", Age=18,
            Email="tilda.svensson@example.com",    Phone="070-678 90 13",
            Address="Trädgårdsgatan 5, 352 36 Växjö",
            SmsReminder=true,  CreatedAt=new DateTime(2024,3,3),   CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/62.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 6, "Korsningar är fortfarande ett svårt moment.", 4)),
            TheoryStepsByCategory=TheoryCat(
                ("B", 5, "Väjningsregler behöver repeteras.", 3)) },

        [20] = new() { Id=20, Name="Ulrika Karlsson",  NickName="Ulle",    Gender="Kvinna", Age=31,
            Email="ulrika.karlsson@example.com",   Phone="073-789 01 24",
            Address="Storgatan 1, 721 30 Västerås",
            SmsReminder=true,  CreatedAt=new DateTime(2023,7,15),  CreatedFrom="Importerad",
            PhotoUrl="https://randomuser.me/api/portraits/women/39.jpg",
            LicenseCategories=["B","BE"],
            TrainingStepsByCategory=Cat(
                ("B",  15, "", -1),
                ("BE", 15, "Utmärkt hantering av bil med släp.", 14)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  12, "", -1),
                ("BE", 12, "Lastsäkring och bogsering perfekt.", 10)) },

        // ── Id 21-30 ──────────────────────────────────────────────────────────
        [21] = new() { Id=21, Name="Viktor Persson",   NickName="Viktor",  Gender="Man",    Age=19,
            Email="viktor.persson@example.com",    Phone="070-890 12 35",
            Address="Nedre Husargatan 12, 413 14 Göteborg",
            SmsReminder=true,  CreatedAt=new DateTime(2024,2,1),   CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/5.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 8)),
            TheoryStepsByCategory=TheoryCat(("B", 7)) },

        [22] = new() { Id=22, Name="Wilma Olsson",     NickName="Wilma",   Gender="Kvinna", Age=17,
            Email="wilma.olsson@example.com",      Phone="076-901 23 46",
            Address="Bergsgatan 44, 116 23 Stockholm",
            SmsReminder=true,  CreatedAt=new DateTime(2024,6,25),  CreatedFrom="Optimapro app",
            PhotoUrl="https://randomuser.me/api/portraits/women/15.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 3, "Bra grundläggande förståelse.", 1)),
            TheoryStepsByCategory=TheoryCat(("B", 4)) },

        [23] = new() { Id=23, Name="Axel Martinsson",  NickName="Axel",    Gender="Man",    Age=20,
            Email="axel.martinsson@example.com",   Phone="070-012 34 57",
            Address="Smedsgatan 17, 211 34 Malmö",
            SmsReminder=true,  CreatedAt=new DateTime(2024,4,30),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/49.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 5)),
            TheoryStepsByCategory=TheoryCat(("B", 6)) },

        [24] = new() { Id=24, Name="Bella Claesson",   NickName="Bella",   Gender="Kvinna", Age=18,
            Email="bella.claesson@example.com",    Phone="073-123 45 68",
            Address="Industrigatan 3, 753 31 Uppsala",
            SmsReminder=false, CreatedAt=new DateTime(2024,1,15),  CreatedFrom="Manuellt",
            PhotoUrl="https://randomuser.me/api/portraits/women/71.jpg",
            LicenseCategories=["B","AM"],
            TrainingStepsByCategory=Cat(
                ("B",  11, "Utmärkt på landsvägskörning.", 7),
                ("AM", 8,  "", -1)),
            TheoryStepsByCategory=TheoryCat(
                ("B",  10, "", -1),
                ("AM", 7,  "", -1)) },

        [25] = new() { Id=25, Name="Carl Hedström",    NickName="Carl",    Gender="Man",    Age=24,
            Email="carl.hedstrom@example.com",     Phone="070-234 56 80",
            Address="Lindgatan 8, 582 28 Linköping",
            SmsReminder=true,  CreatedAt=new DateTime(2023,11,20), CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/22.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 13)),
            TheoryStepsByCategory=TheoryCat(("B", 11)) },

        [26] = new() { Id=26, Name="Diana Forsberg",   NickName="Diana",   Gender="Kvinna", Age=16,
            Email="diana.forsberg@example.com",    Phone="076-345 67 91",
            Address="Möllergatan 29, 703 61 Örebro",
            SmsReminder=true,  CreatedAt=new DateTime(2024,7,1),   CreatedFrom="Optimapro app",
            PhotoUrl="https://randomuser.me/api/portraits/women/91.jpg",
            LicenseCategories=["AM"],
            TrainingStepsByCategory=Cat(
                ("AM", 4, "Behöver mer tid på moped.", 3)),
            TheoryStepsByCategory=TheoryCat(("AM", 5)) },

        [27] = new() { Id=27, Name="Elias Sundqvist",  NickName="Elias",   Gender="Man",    Age=21,
            Email="elias.sundqvist@example.com",   Phone="070-456 78 92",
            Address="Stallgatan 6, 602 22 Norrköping",
            SmsReminder=true,  CreatedAt=new DateTime(2024,5,14),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/men/61.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 9)),
            TheoryStepsByCategory=TheoryCat(("B", 8)) },

        [28] = new() { Id=28, Name="Frida Björk",      NickName="Frida",   Gender="Kvinna", Age=19,
            Email="frida.bjork@example.com",       Phone="073-567 89 03",
            Address="Hantverksgatan 11, 252 46 Helsingborg",
            SmsReminder=true,  CreatedAt=new DateTime(2024,3,8),   CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/53.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 2)),
            TheoryStepsByCategory=TheoryCat(("B", 3)) },

        [29] = new() { Id=29, Name="Gustav Åberg",     NickName="Gustav",  Gender="Man",    Age=23,
            Email="gustav.aberg@example.com",      Phone="070-678 90 14",
            Address="Kyrkogatan 55, 553 15 Jönköping",
            SmsReminder=false, CreatedAt=new DateTime(2023,10,10), CreatedFrom="Importerad",
            PhotoUrl="https://randomuser.me/api/portraits/men/81.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(
                ("B", 7, "God rumsuppfattning vid parkering.", 12)),
            TheoryStepsByCategory=TheoryCat(("B", 6)) },

        [30] = new() { Id=30, Name="Hannah Lindgren",  NickName="Hannah",  Gender="Kvinna", Age=22,
            Email="hannah.lindgren@example.com",   Phone="076-789 01 25",
            Address="Bredgatan 4, 903 27 Umeå",
            SmsReminder=true,  CreatedAt=new DateTime(2024,2,20),  CreatedFrom="Optimapro webb",
            PhotoUrl="https://randomuser.me/api/portraits/women/29.jpg",
            LicenseCategories=["B"],
            TrainingStepsByCategory=Cat(("B", 12)),
            TheoryStepsByCategory=TheoryCat(("B", 11)) },
    };

    /// <summary>Look up a student profile by the student's name (case-insensitive).
    /// Falls back to a generated profile for unknown names.</summary>
    public StudentProfile GetStudentProfile(string name)
    {
        var student = Students.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (student != null && _studentProfiles.TryGetValue(student.Id, out var profile))
            return profile;

        // Check named profiles (contacts/relatives not in main student list)
        if (_namedProfiles.TryGetValue(name, out var namedProfile))
            return namedProfile;

        // Generate a generic profile on the fly and cache it
        var id = student?.Id ?? Math.Abs(name.GetHashCode() % 10000) + 100;
        if (!_studentProfiles.TryGetValue(id, out var generic))
        {
            var isFemale = name.Split(' ').FirstOrDefault()?.EndsWith("a") == true;
            var avatarNum = (Math.Abs(name.GetHashCode()) % 70) + 1;
            var firstName = name.Split(' ').FirstOrDefault() ?? name;
            generic = new StudentProfile
            {
                Id = id, Name = name,
                NickName    = firstName,
                Gender      = isFemale ? "Kvinna" : "Man",
                Age         = 18 + (id % 12),
                Address     = $"Storgatan {id % 99 + 1}, 123 45 Stad",
                SmsReminder = (id % 5) != 0,
                CreatedAt   = new DateTime(2024, (id % 12) + 1, (id % 28) + 1),
                CreatedFrom = (id % 3) switch { 0 => "Manuellt", 1 => "Optimapro app", _ => "Optimapro webb" },
                Email = $"{name.ToLower().Replace(" ", ".").Replace("å","a").Replace("ä","a").Replace("ö","o")}@example.com",
                Phone = $"07{id % 10}-{(id * 7 % 900) + 100} {(id * 13 % 90) + 10} {(id * 17 % 90) + 10}",
                PhotoUrl = $"https://randomuser.me/api/portraits/{(isFemale ? "women" : "men")}/{avatarNum}.jpg",
                LicenseCategories = ["B"],
                TrainingStepsByCategory = Cat(("B", 0)),
                TheoryStepsByCategory = TheoryCat(("B", 0))
            };
            _studentProfiles[id] = generic;
        }
        return generic;
    }

    public List<PickupLocation> PickupLocations { get; } = new()
    {
        new() { Id = 1,  Name = "Centralstationen",       Address = "Vasagatan 1"            },
        new() { Id = 2,  Name = "Hemma hos eleven",       Address = "(elevens hemadress)"    },
        new() { Id = 3,  Name = "Trafikskolan",           Address = "Körskolevägen 5"        },
        new() { Id = 4,  Name = "Köpcentrum Cityhallen",  Address = "Storgatan 14"           },
        new() { Id = 5,  Name = "Gymnasieskolan",         Address = "Skolvägen 2"            },
        new() { Id = 6,  Name = "Universitetet",          Address = "Campusvägen 10"         },
        new() { Id = 7,  Name = "Idrottshallen",          Address = "Idrottsvägen 3"         },
        new() { Id = 8,  Name = "Trafikverket",           Address = "Provvägen 1"            },
    };

    public List<Resource> Resources { get; } = new()
    {
        new() { Id = 1,  Name = "Volvo V60 (ARN 123)",  Type = ResourceType.Car,       IsAvailable = true  },
        new() { Id = 2,  Name = "Ford Focus (BCK 456)",  Type = ResourceType.Car,       IsAvailable = true  },
        new() { Id = 3,  Name = "Opel Astra (CDF 789)", Type = ResourceType.Car,       IsAvailable = true  },
        new() { Id = 4,  Name = "Toyota Corolla (EGH 012)", Type = ResourceType.Car,   IsAvailable = false },
        new() { Id = 5,  Name = "Sal 1",                Type = ResourceType.Classroom, IsAvailable = true  },
        new() { Id = 6,  Name = "Sal 2",                Type = ResourceType.Classroom, IsAvailable = true  },
        new() { Id = 7,  Name = "Simulator",            Type = ResourceType.Simulator, IsAvailable = true  },
    };

    private List<CalendarEvent> _events = new();
    private int _nextEventId = 1000;

    public BookingService()
    {
        _studentProfiles = BuildProfiles();

        // Assign education plans with varying mock completion states
        // Total sub-items after splitting = 58
        var doneCounts = new int[] { 16,10,58,24,0,6,36,14,44,4, 28,18,50,2,34,12,40,8,22,54, 30,16,6,38,20,48,0,26,12,42 };
        int pi = 1;
        foreach (var kv in _studentProfiles)
        {
            kv.Value.EduPlan            = MakeEduPlan(doneCounts[(pi++ - 1) % doneCounts.Length]);
            kv.Value.TheoryAreaProgress = MakeTheoryAreas(kv.Key);
            kv.Value.ContactPersons     = MakeContacts(kv.Key, kv.Value.Name, kv.Value.Age);
        }

        // Override Alice Bergström's contacts with specific named parent profiles
        _studentProfiles[1].ContactPersons = new List<ContactPerson>
        {
            new() { Name = "Maria Bergström", Relation = "Mamma", Phone = "070-234 56 78" },
            new() { Name = "Lars Bergström",  Relation = "Pappa",  Phone = "073-456 78 90" },
        };

        GenerateMockEvents();
    }

    // ── Education plan (Förarbevis – Moped) ──────────────────────────────────

    private static EduPlan MakeEduPlan(int doneCount = 0)
    {
        // All leaf items in order as (sectionTitle, sectionMark, itemTitle, itemMark, globalId)
        var raw = new (string Sec, string SecMark, string Item, string Mark, string Gid)[]
        {
            ("Informationslektion",          "",       "Information",                                                                                                      "",       "51db5b27-4882-44cf-b2da-67fa258e2928"),
            ("1 Körstállning",               "Mom 1",  "1 a klädsel och körstállning, 1 b reglage och instrument",                                                        "ab",     "8aefff06-cc2c-4e7d-99e2-ab409092e888"),
            ("2 Inledande manövrering",      "Mom 2",  "2 a start och stannande",                                                                                          "a",      "c01c2716-337b-47a3-abe3-ff2eb0c69ba3"),
            ("2 Inledande manövrering",      "Mom 2",  "2 b krypkörning och styrning",                                                                                     "b",      "b8e367d4-bb4c-4ea0-9f97-3fc732dc8625"),
            ("3 Växling",                    "Mom 3",  "3 a uppväxling, 3 b bromsning, 3 c nedväxling",                                                                    "abc",    "e704aaa6-34fd-4428-8100-99ef302364b1"),
            ("4 Lutning",                    "Mom 4",  "4 a motlut, 4 b medlut",                                                                                           "ab",     "0fbe8f7a-73a4-459f-8848-4510b55807cc"),
            ("5 Manövrering",                "Mom 5",  "5 a låg fart, 5 b hög fart, 5 c acceleration, 5 d hård bromsning",                                                "abcd",   "7cf5d1af-df56-45af-8cc0-5a8bfdd9d316"),
            ("6 Funktion och kontroll",      "Mom 6",  "6 a mopeden",                                                                                                      "a",      "c3bcc539-c96a-4221-b3cd-f27c2926be62"),
            ("6 Funktion och kontroll",      "Mom 6",  "6 b passagerare och last, 6 c släp",                                                                               "bc",     "70e51338-26b8-4863-b29b-f205ec886964"),
            ("6 Funktion och kontroll",      "Mom 6",  "6 d säkerhetskontroll",                                                                                            "d",      "1f8898b5-9d72-46e7-b2d5-adb4ca8772e4"),
            ("Människa",                     "",       "Människa",                                                                                                         "",       "4ad52141-5874-4151-bc1a-d0e02283245c"),
            ("Miljön",                       "",       "Miljön",                                                                                                           "",       "95bd2655-7477-4b09-986b-0e01c7e6b698"),
            ("7 Samordning och bromsning",   "Mom 7",  "7 a avsökning och riskbedömning, 7 b samordning och motorik",                                                      "ab",     "d8173a97-1527-4dc5-b77f-8052a3904dc1"),
            ("8 Mindre samhälle",            "Mom 8",  "8 a avsökning och riskbedömning, 8 b hastighetsanpassning, 8 c placering",                                         "abc",    "a8150b06-2f30-4689-95e4-bf4c58c6cdff"),
            ("8 Mindre samhälle",            "Mom 8",  "8 d väjningsregler",                                                                                               "d",      "f1c25bc5-3e7a-4c2a-9804-79137f67f235"),
            ("9 Mindre landsväg",            "Mom 9",  "9 a avsökning och riskbedömning, 9 b hastighetsanpassning, 9 c placering",                                         "abc",    "320f45ed-0e4e-4da7-80cb-f9bb25386fa4"),
            ("9 Mindre landsväg",            "Mom 9",  "9 d väjningsregler",                                                                                               "d",      "ced62f5f-4df5-4e11-a5e5-7e5999c9bbee"),
            ("9 Mindre landsväg",            "Mom 9",  "9 e järnvägskorsning",                                                                                             "e",      "efe54055-f75e-4b6a-b55e-e4887d5cf837"),
            ("10 Stad",                      "Mom 10", "10 a avsökning och riskbedömning, 10 b hastighetsanpassning, 10 c placering",                                      "abc",    "21b48419-26bc-42a9-bc79-d09e8faf2e58"),
            ("10 Stad",                      "Mom 10", "10 d väjningsregler",                                                                                              "d",      "8e5a7524-a963-4663-84b1-fa3bd2e490a9"),
            ("10 Stad",                      "Mom 10", "10 e trafiksignal",                                                                                                "e",      "8c052992-1427-4792-83ce-f353efc2296c"),
            ("10 Stad",                      "Mom 10", "10 f enkelriktad gata",                                                                                            "f",      "0ba163f6-11bf-4d39-ae18-14e5405d6299"),
            ("10 Stad",                      "Mom 10", "10 g cirkulationsplats",                                                                                           "g",      "2a0cdac6-632d-4722-b980-35d3c37c233b"),
            ("10 Stad",                      "Mom 10", "10 h vändning och parkering",                                                                                      "h",      "7c482a48-f6d1-4964-b737-86459ae1ef22"),
            ("11 Landsväg",                  "Mom 11", "11 a avsökning och riskbedömning, 11 b hastighetsanpassning, 11 c placering",                                      "abc",    "226843f0-8297-4bb2-8b66-64ae7c891a64"),
            ("11 Landsväg",                  "Mom 11", "11 d påfart och avfart, 11 e omkörning, 11 f vändning och parkering",                                              "def",    "f6908d5c-d545-4bc7-9065-5a69e8a23250"),
            ("12 Höghastighetsväg",          "Mom 12", "12 a avsökning och riskbedömning, 12 b hastighetsanpassning, 12 c motorväg, 12 d motortrafikled, 12 e mitträckesväg","abcde", "e79459b0-b3a6-48e7-92f2-14e8f33c0be2"),
            ("13 Mörker",                    "Mom 13", "13 a avsökning och riskbedömning, 13 b hastighetsanpassning, 13 c mörkerdemonstration, 13 d möte, 13 e omkörning, 13 f parkering, 13 g nedsatt sikt", "abcdefg", "30c1056f-caf9-4b08-9358-bcc44afffc8d"),
            ("14 Halt väglag",               "Mom 14", "14 a olika typer av halka, 14 b utrustning och hjälpsystem",                                                       "ab",     "add12ee3-862e-489f-a278-3a5c4ea9eb44"),
        };

        var plan = new EduPlan { Title = "Förarbevis – Moped" };
        EduPlanSection? current = null;
        int subIdx = 0; // global index across all split sub-items

        foreach (var r in raw)
        {
            // Start a new section when sec title changes
            if (current == null || current.Title != r.Sec)
            {
                current = new EduPlanSection { Title = r.Sec, Mark = r.SecMark };
                plan.Sections.Add(current);
            }

            // Split compound titles like "3 a uppväxling, 3 b bromsning, 3 c nedväxling"
            var parts = SplitMoments(r.Item);
            for (int p = 0; p < parts.Count; p++)
            {
                current.Items.Add(new EduPlanItem
                {
                    GlobalId = parts.Count == 1 ? r.Gid : $"{r.Gid}_{p}",
                    Title    = parts[p],
                    Done     = subIdx < doneCount,
                });
                subIdx++;
            }
        }
        return plan;
    }

    /// <summary>
    /// Splits a compound moment title such as
    /// "3 a uppväxling, 3 b bromsning, 3 c nedväxling" into
    /// ["3 a uppväxling", "3 b bromsning", "3 c nedväxling"].
    /// Single-part titles are returned as-is.
    /// </summary>
    private static List<string> SplitMoments(string title)
    {
        // Split at ", N letter " pattern (e.g. ", 3 b ", ", 13 f ")
        var parts = System.Text.RegularExpressions.Regex
            .Split(title, @",\s+(?=\d+\s+[a-z]\s)")
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();
        return parts.Count > 0 ? parts : new List<string> { title };
    }

    // ── Theory area progress ──────────────────────────────────────────────────

    private static readonly string[] TheoryAreaNames =
    {
        "Trafikgrunder",            "Körklar",                    "Körteknik",
        "Manövrering",              "Säker förare",               "Säker trafik – grund",
        "Fordonskännedom",          "Lasta och dra",              "Förarstöd",
        "Att äga ett fordon",       "Säkerhetskontroll",          "Miljö- och klimatpåverkan",
        "Oskyddade trafikanter",    "Anvisningar och vägmärken",  "Väjningsregler",
        "Körfält",                  "Trafiksituationer i stad",   "Säker trafik – stad",
        "Stanna och parkera",       "Naturlagar",                 "Olyckor",
        "Mindre landsväg",          "Trafiksituationer på landsväg", "Säker trafik – landsväg",
        "Högfartsväg",              "Yttre omständigheter",
    };

    /// <summary>
    /// Generates seeded-random theory area completion percentages for a student.
    /// The baseline is derived from the student's id so each student gets a
    /// <summary>Generates seeded-random contact persons for a student.</summary>
    private static List<ContactPerson> MakeContacts(int id, string studentName, int age)
    {
        // Swedish first names for parents/contacts
        string[] mammaNames = ["Maria","Anna","Eva","Karin","Sara","Lena","Christina","Emma","Sofia","Ingrid"];
        string[] pappaNames = ["Lars","Erik","Johan","Anders","Per","Karl","Thomas","Stefan","Mikael","Peter"];
        string[] partnerNames = ["Johanna","Linda","Sandra","Monica","Peter","Marcus","Daniel","Robert"];

        var rng      = new Random(id * 17 + 3);
        var lastName = studentName.Contains(' ') ? studentName.Split(' ')[^1] : studentName;
        var contacts = new List<ContactPerson>();

        if (age < 25)
        {
            // Younger students: one or two parents
            contacts.Add(new ContactPerson
            {
                Name     = $"{mammaNames[rng.Next(mammaNames.Length)]} {lastName}",
                Relation = "Mamma",
                Phone    = $"07{rng.Next(0,4)}-{rng.Next(100,999)} {rng.Next(10,99)} {rng.Next(10,99)}"
            });
            if (rng.Next(2) == 0)
            {
                contacts.Add(new ContactPerson
                {
                    Name     = $"{pappaNames[rng.Next(pappaNames.Length)]} {lastName}",
                    Relation = "Pappa",
                    Phone    = $"07{rng.Next(0,4)}-{rng.Next(100,999)} {rng.Next(10,99)} {rng.Next(10,99)}"
                });
            }
        }
        else
        {
            // Older students: partner or sibling
            bool hasPartner = rng.Next(2) == 0;
            contacts.Add(new ContactPerson
            {
                Name     = $"{partnerNames[rng.Next(partnerNames.Length)]} {lastName}",
                Relation = hasPartner ? "Partner" : "Syskon",
                Phone    = $"07{rng.Next(0,4)}-{rng.Next(100,999)} {rng.Next(10,99)} {rng.Next(10,99)}"
            });
        }
        return contacts;
    }

    /// consistent but unique spread across the 26 areas.
    /// </summary>
    private static List<Models.TheoryAreaProgress> MakeTheoryAreas(int studentId)
    {
        // Use the student id as a seed so results are stable across page reloads.
        var rng      = new Random(studentId * 31 + 7);
        // Baseline 0–100 % — students with lower ids tend to be less advanced.
        int baseline = Math.Clamp((studentId % 10) * 10 + rng.Next(-10, 20), 0, 100);

        return TheoryAreaNames.Select(name => new Models.TheoryAreaProgress
        {
            Name       = name,
            Percentage = Math.Clamp(baseline + rng.Next(-35, 36), 0, 100),
        }).ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// Maps a free slot's time/duration to a suitable lesson type ID.
    private static int FreeLessonType(int sh, int sm, int eh, int em)
    {
        int durationMin = (eh * 60 + em) - (sh * 60 + sm);
        if (durationMin >= 120) return 5;   // Riskutbildning 1
        if (durationMin >= 90)  return 3;   // Motorvägslektion
        if (sh >= 17)           return 4;   // Körlektion kväll
        if (durationMin <= 50)  return 1;   // Introduktionslektion
        return 2;                           // Körlektion B
    }

    // ── Mock data generation ──────────────────────────────────────────────────

    private void GenerateMockEvents()
    {
        var today = DateTime.Today;
        // Anchor to Monday of the current week
        var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday) monday = monday.AddDays(-7);

        int id = 1;

        // ── Teacher 1: Anna Lindgren ─────────────────────────────────────────
        // Lediga pass styrs av hennes aktiva lektionsmall (GenerateTemplateSlots).
        // Här läggs bara in riktiga bokningar som finns i systemet.

        // ── Teacher 2: Erik Svensson ─────────────────────────────────────────
        foreach (var (dayOffset, slots) in new[]
        {
            (0, new[] { (9,0,10,0,true,"Per Lund"), (10,0,11,0,false,""), (12,0,14,0,false,""), (14,0,15,0,true,"Hanna Ros"), (17,0,19,0,false,"") }),
            (1, new[] { (7,0,9,0,false,""), (9,0,10,0,true,"Emil Gran"), (11,0,13,0,false,""), (15,0,16,0,true,"Klara Vik") }),
            (2, new[] { (8,0,9,0,false,""), (10,0,11,0,true,"Axel Bom"), (12,0,13,30,false,""), (13,30,14,30,true,"Vera Lund"), (15,0,17,0,false,"") }),
            (3, new[] { (7,0,9,0,false,""), (9,0,10,0,true,"Liam Ros"), (13,0,14,0,false,""), (14,0,15,0,true,"Freja Dal") }),
            (4, new[] { (8,0,10,0,false,""), (10,0,11,0,true,"Hugo Sten"), (14,0,16,0,false,"") }),
        })
        {
            var day = monday.AddDays(dayOffset);
            foreach (var (sh, sm, eh, em, booked, student) in slots)
            {
                _events.Add(new CalendarEvent
                {
                    Id = id++, TeacherId = 2,
                    StartTime = day.AddHours(sh).AddMinutes(sm),
                    EndTime   = day.AddHours(eh).AddMinutes(em),
                    IsBooked  = booked,
                    StudentName = student,
                    LessonTypeId = booked ? (id % 5 + 1) : FreeLessonType(sh, sm, eh, em),
                    ResourceIds  = booked ? new List<int> { (id % 3) + 1 } : new(),
                });
            }
        }

        // ── Teacher 3: Maria Johansson ──────────────────────────────────────
        foreach (var (dayOffset, slots) in new[]
        {
            (0, new[] { (8,0,9,0,false,""), (9,0,10,0,true,"Astrid Kron"), (12,0,13,0,false,""), (15,0,16,0,false,""), (17,0,19,0,false,"") }),
            (1, new[] { (7,0,9,0,false,""), (9,0,10,0,true,"Oskar Holm"), (10,0,11,0,false,""), (13,0,14,30,true,"Tuva Berg"), (14,30,15,30,false,"") }),
            (2, new[] { (8,0,10,0,false,""), (10,0,11,0,true,"Elsa Vik"), (12,0,13,0,false,""), (15,0,16,0,true,"Olle Gran") }),
            (3, new[] { (9,0,10,0,false,""), (10,0,11,0,true,"Sigge Dal"), (13,0,15,0,false,"") }),
            (4, new[] { (8,0,9,0,false,""), (9,0,10,0,true,"Iris Lund"), (11,0,13,0,false,""), (14,0,16,0,false,"") }),
            (5, new[] { (9,0,10,0,false,""), (10,0,11,0,true,"Milo Ros"), (11,0,12,0,false,"") }),
            (6, new[] { (10,0,11,0,false,""), (11,0,12,0,true,"Lova Sten"), (12,0,13,0,false,"") }),
        })
        {
            var day = monday.AddDays(dayOffset);
            foreach (var (sh, sm, eh, em, booked, student) in slots)
            {
                _events.Add(new CalendarEvent
                {
                    Id = id++, TeacherId = 3,
                    StartTime = day.AddHours(sh).AddMinutes(sm),
                    EndTime   = day.AddHours(eh).AddMinutes(em),
                    IsBooked  = booked,
                    StudentName = student,
                    LessonTypeId = booked ? (id % 6 + 1) : FreeLessonType(sh, sm, eh, em),
                    ResourceIds  = booked ? new List<int> { (id % 3) + 1 } : new(),
                });
            }
        }

        // ── Teachers 4–6: Johan, Petra, Lars (less dense) ───────────────────
        var t4Slots = new (int teacher, int dayOff, int sh, int sm, int eh, int em, bool booked, string student)[]
        {
            (4,0,9,0,10,0,true,"Ben Kron"), (4,0,10,0,11,0,false,""), (4,0,13,0,14,0,true,"Fia Dal"), (4,1,8,0,9,0,false,""), (4,1,9,0,10,0,true,"Jon Vik"),
            (4,2,10,0,11,0,false,""), (4,3,9,0,10,0,true,"Tilda Sten"), (4,4,11,0,12,0,false,""), (4,4,14,0,15,0,true,"Alva Ros"),
            (5,0,8,0,9,0,false,""), (5,0,10,0,11,0,true,"Gustav Berg"), (5,1,9,0,10,0,false,""), (5,2,8,0,10,0,true,"Ebba Holm"), (5,3,13,0,14,0,false,""), (5,4,9,0,10,0,true,"Hugo Gren"),
            (6,0,7,0,9,0,false,""), (6,0,9,0,10,0,true,"Stina Lund"), (6,1,10,0,11,0,false,""), (6,2,9,0,10,0,true,"Max Bom"), (6,3,8,0,9,0,false,""), (6,4,10,0,11,0,true,"Nova Ros"),
        };

        foreach (var (teacher, dayOff, sh, sm, eh, em, booked, student) in t4Slots)
        {
            var day = monday.AddDays(dayOff);
            _events.Add(new CalendarEvent
            {
                Id = id++, TeacherId = teacher,
                StartTime = day.AddHours(sh).AddMinutes(sm),
                EndTime   = day.AddHours(eh).AddMinutes(em),
                IsBooked  = booked,
                StudentName = student,
                LessonTypeId = booked ? (id % 7 + 1) : FreeLessonType(sh, sm, eh, em),
                ResourceIds  = booked ? new List<int> { (id % 3) + 1 } : new(),
            });
        }

        // Introductionslecture next week as well (week + 7) – ej Anna, hon styrs av mall
        foreach (var t in new[] { 2, 3 })
        {
            var nextMon = monday.AddDays(7);
            for (int d = 0; d < 5; d++)
            {
                var day = nextMon.AddDays(d);
                _events.Add(new() { Id = id++, TeacherId = t, StartTime = day.AddHours(8),  EndTime = day.AddHours(10), IsBooked = false, LessonTypeId = FreeLessonType(8,  0, 10, 0) });
                _events.Add(new() { Id = id++, TeacherId = t, StartTime = day.AddHours(13), EndTime = day.AddHours(14), IsBooked = false, LessonTypeId = FreeLessonType(13, 0, 14, 0) });
            }
        }

        // ── Delade lektioner (typ 8) ─────────────────────────────────────────
        _events.Add(new CalendarEvent
        {
            Id = id++, TeacherId = 1,
            StartTime    = monday.AddDays(1).AddHours(14),
            EndTime      = monday.AddDays(1).AddHours(16),
            IsBooked     = true,
            StudentName  = "Alice Bergström",
            ExtraStudents = new List<string> { "Björn Lindqvist", "Cecilia Holm" },
            LessonTypeId = 8,
            ResourceIds  = new List<int> { 1 },
        });
        _events.Add(new CalendarEvent
        {
            Id = id++, TeacherId = 2,
            StartTime    = monday.AddDays(3).AddHours(10),
            EndTime      = monday.AddDays(3).AddHours(11),
            IsBooked     = true,
            StudentName  = "David Ekström",
            ExtraStudents = new List<string> { "Emma Sjöberg" },
            LessonTypeId = 8,
            ResourceIds  = new List<int> { 2 },
        });
        _events.Add(new CalendarEvent
        {
            Id = id++, TeacherId = 3,
            StartTime    = monday.AddDays(2).AddHours(15),
            EndTime      = monday.AddDays(2).AddHours(16),
            IsBooked     = true,
            StudentName  = "Filip Gustafsson",
            ExtraStudents = new List<string> { "Gabriella Nordin", "Henrik Sandberg", "Ida Wallin" },
            LessonTypeId = 8,
            ResourceIds  = new List<int> { 3 },
        });

        // ── Riskutbildning-sessioner (typ 5 & 6, upp till 20 elever) ────────────

        // Riskutbildning 1 – tisdag nästa vecka, 08:00–10:00, 7 bokade av 20
        _events.Add(new CalendarEvent
        {
            Id = id++, TeacherId = 1,
            StartTime    = monday.AddDays(8).AddHours(8),
            EndTime      = monday.AddDays(8).AddHours(10),
            IsBooked     = true,
            LessonTypeId = 5,
            StudentName  = "Alice Bergström",
            ExtraStudents = new List<string>
            {
                "Björn Lindqvist", "Cecilia Holm", "David Ekström",
                "Emma Sjöberg",    "Filip Gustafsson", "Gabriella Nordin",
            },
            ResourceIds = new List<int> { 4 },
        });

        // Riskutbildning 1 – torsdag nästa vecka, 13:00–15:00, 12 bokade av 20
        _events.Add(new CalendarEvent
        {
            Id = id++, TeacherId = 2,
            StartTime    = monday.AddDays(10).AddHours(13),
            EndTime      = monday.AddDays(10).AddHours(15),
            IsBooked     = true,
            LessonTypeId = 5,
            StudentName  = "Henrik Sandberg",
            ExtraStudents = new List<string>
            {
                "Ida Wallin",    "Johan Lindberg",  "Klara Persson",
                "Linus Ek",      "Maria Jonsson",   "Nils Eriksson",
                "Olivia Svensson","Pontus Karlsson", "Rebecka Nilsson",
                "Stefan Åberg",  "Tina Magnusson",
            },
            ResourceIds = new List<int> { 4 },
        });

        // Riskutbildning 2 – onsdag innevarande vecka, 09:00–10:30, 18 bokade av 20
        _events.Add(new CalendarEvent
        {
            Id = id++, TeacherId = 3,
            StartTime    = monday.AddDays(2).AddHours(9),
            EndTime      = monday.AddDays(2).AddHours(10).AddMinutes(30),
            IsBooked     = true,
            LessonTypeId = 6,
            StudentName  = "Ulrika Forssberg",
            ExtraStudents = new List<string>
            {
                "Viktor Holmgren",  "Wilda Johansson", "Xerxes Patel",
                "Ylva Lindström",   "Zelda Moberg",    "Adam Bergqvist",
                "Bella Carlsson",   "Carl Dahl",       "Diana Engström",
                "Erika Falk",       "Fredrik Gran",    "Hanna Holm",
                "Isak Ivarsson",    "Julia Jansson",   "Karl Kronqvist",
                "Lisa Lundgren",    "Marcus Melin",
            },
            ResourceIds = new List<int> { 4 },
        });

        // ── Teachers 7–11: Grupp 2 (sparse events) ──────────────────────────
        var grp2Slots = new (int teacher, int dayOff, int sh, int sm, int eh, int em, bool booked, string student)[]
        {
            ( 7,0, 9,0,10,0,true, "Petra Sjö"),  ( 7,0,11,0,12,0,false,""), ( 7,1, 8,0,9,0,false,""), ( 7,1, 9,0,10,0,true,"Leo Kron"),
            ( 7,2,10,0,11,0,false,""),            ( 7,3, 9,0,10,0,true,"Stella Vik"), ( 7,4,13,0,14,0,false,""),
            ( 8,0, 8,0,9,0,false,""),             ( 8,0,10,0,11,0,true,"Albin Dal"),  ( 8,1, 9,0,10,0,false,""), ( 8,2, 8,0,10,0,true,"Maja Bom"),
            ( 8,3,13,0,14,0,false,""),            ( 8,4, 9,0,10,0,true,"Elin Ros"),
            ( 9,0, 9,0,10,0,true,"Filip Sten"),  ( 9,1,10,0,11,0,false,""), ( 9,2, 9,0,10,0,false,""), ( 9,3, 8,0,9,0,true,"Saga Lund"),
            ( 9,4,11,0,12,0,false,""),
            (10,0, 8,0,9,0,false,""),             (10,0,10,0,11,0,true,"Otto Gren"),  (10,1, 9,0,10,0,true,"Ines Holm"), (10,2,10,0,11,0,false,""),
            (10,3, 9,0,10,0,true,"Noel Berg"),    (10,4,13,0,14,0,false,""),
            (11,0, 9,0,10,0,true,"Vera Vik"),     (11,1, 8,0,9,0,false,""),  (11,1,10,0,11,0,true,"Karl Dal"),  (11,2,11,0,12,0,false,""),
            (11,3,14,0,15,0,true,"Siri Ros"),     (11,4, 9,0,10,0,false,""),
        };

        foreach (var (teacher, dayOff, sh, sm, eh, em, booked, student) in grp2Slots)
        {
            var day = monday.AddDays(dayOff);
            _events.Add(new CalendarEvent
            {
                Id = id++, TeacherId = teacher,
                StartTime    = day.AddHours(sh).AddMinutes(sm),
                EndTime      = day.AddHours(eh).AddMinutes(em),
                IsBooked     = booked,
                StudentName  = student,
                LessonTypeId = booked ? (id % 7 + 1) : FreeLessonType(sh, sm, eh, em),
                ResourceIds  = booked ? new List<int> { (id % 3) + 1 } : new(),
            });
        }

        // ── Past körlektion events for Alice Bergström (Utbildning-demo) ────────
        // These sit in the past so they show under "Tidigare timmar"
        var alice = "Alice Bergström";
        var pastDays = new[] { -28, -21, -14, -7 };
        var pastMoments = new[]
        {
            // Lektion 1 (-28 days): Information + 1a klädsel + 1b reglage
            new List<LessonMoment>
            {
                new() { GlobalId = "51db5b27-4882-44cf-b2da-67fa258e2928",   Title = "Information",              Score = 5, Comment = "Bra genomgång av grunderna." },
                new() { GlobalId = "8aefff06-cc2c-4e7d-99e2-ab409092e888_0", Title = "1 a klädsel och körstállning", Score = 3, Comment = "" },
                new() { GlobalId = "8aefff06-cc2c-4e7d-99e2-ab409092e888_1", Title = "1 b reglage och instrument",    Score = 2, Comment = "Behöver öva mer på instrument." },
            },
            // Lektion 2 (-21 days): 2a start + 2b krypkörning + 3a uppväxling
            new List<LessonMoment>
            {
                new() { GlobalId = "c01c2716-337b-47a3-abe3-ff2eb0c69ba3",   Title = "2 a start och stannande",    Score = 4, Comment = "" },
                new() { GlobalId = "b8e367d4-bb4c-4ea0-9f97-3fc732dc8625",   Title = "2 b krypkörning och styrning", Score = 3, Comment = "Krypkörning bra, styrning kan förbättras." },
                new() { GlobalId = "e704aaa6-34fd-4428-8100-99ef302364b1_0", Title = "3 a uppväxling",              Score = 2, Comment = "" },
            },
            // Lektion 3 (-14 days): 3b bromsning + 3c nedväxling + 4a motlut
            new List<LessonMoment>
            {
                new() { GlobalId = "e704aaa6-34fd-4428-8100-99ef302364b1_1", Title = "3 b bromsning",   Score = 4, Comment = "Mycket bra bromsning." },
                new() { GlobalId = "e704aaa6-34fd-4428-8100-99ef302364b1_2", Title = "3 c nedväxling",  Score = 3, Comment = "" },
                new() { GlobalId = "0fbe8f7a-73a4-459f-8848-4510b55807cc_0", Title = "4 a motlut",      Score = 1, Comment = "Svårt med motlut – behöver mer träning." },
            },
            // Lektion 4 (-7 days): 4b medlut + 5a låg fart + 5b hög fart
            new List<LessonMoment>
            {
                new() { GlobalId = "0fbe8f7a-73a4-459f-8848-4510b55807cc_1", Title = "4 b medlut",      Score = 3, Comment = "" },
                new() { GlobalId = "7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_0", Title = "5 a låg fart",    Score = 3, Comment = "" },
                new() { GlobalId = "7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_1", Title = "5 b hög fart",    Score = 2, Comment = "Behöver bli säkrare i höga hastigheter." },
            },
        };

        for (int pi = 0; pi < pastDays.Length; pi++)
        {
            _events.Add(new CalendarEvent
            {
                Id           = id++,
                TeacherId    = 1,
                StartTime    = today.AddDays(pastDays[pi]).AddHours(9),
                EndTime      = today.AddDays(pastDays[pi]).AddHours(10),
                IsBooked     = true,
                StudentName  = alice,
                LessonTypeId = 2,
                ResourceIds  = new List<int> { 1 },
                LinkedMoments = pastMoments[pi],
            });
        }

        // Extra past events for Alice – demonstrates "Behöver åtgärd" logic
        // -35 days: no moments at all → triggers "needs attention"
        _events.Add(new CalendarEvent
        {
            Id           = id++,
            TeacherId    = 1,
            StartTime    = today.AddDays(-35).AddHours(9),
            EndTime      = today.AddDays(-35).AddHours(10),
            IsBooked     = true,
            StudentName  = alice,
            LessonTypeId = 2,
            ResourceIds  = new List<int> { 2 },
            LinkedMoments = new List<LessonMoment>(), // intentionally empty
        });
        // -42 days: one non-approved (Score=1) moment WITHOUT comment → triggers "needs attention"
        _events.Add(new CalendarEvent
        {
            Id           = id++,
            TeacherId    = 1,
            StartTime    = today.AddDays(-42).AddHours(13),
            EndTime      = today.AddDays(-42).AddHours(14),
            IsBooked     = true,
            StudentName  = alice,
            LessonTypeId = 2,
            ResourceIds  = new List<int> { 1 },
            LinkedMoments = new List<LessonMoment>
            {
                new() { GlobalId = "7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_2", Title = "5 c acceleration",  Score = 5, Comment = "Utmärkt acceleration." },
                new() { GlobalId = "7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_3", Title = "5 d hård bromsning", Score = 1, Comment = "" }, // no comment → needs attention
            },
        });
        // -49 days: all moments approved with comments → does NOT trigger "needs attention"
        _events.Add(new CalendarEvent
        {
            Id           = id++,
            TeacherId    = 2,
            StartTime    = today.AddDays(-49).AddHours(10),
            EndTime      = today.AddDays(-49).AddHours(11),
            IsBooked     = true,
            StudentName  = alice,
            LessonTypeId = 2,
            ResourceIds  = new List<int> { 2 },
            LinkedMoments = new List<LessonMoment>
            {
                new() { GlobalId = "c3bcc539-c96a-4221-b3cd-f27c2926be62",   Title = "6 a mopeden",            Score = 4, Comment = "Bra hantering av korsningar." },
                new() { GlobalId = "70e51338-26b8-4863-b29b-f205ec886964_0", Title = "6 b passagerare och last", Score = 4, Comment = "" },
            },
        });

        // One upcoming körlektion for Alice (next week)
        _events.Add(new CalendarEvent
        {
            Id           = id++,
            TeacherId    = 1,
            StartTime    = today.AddDays(5).AddHours(10),
            EndTime      = today.AddDays(5).AddHours(11),
            IsBooked     = true,
            StudentName  = alice,
            LessonTypeId = 2,
            ResourceIds  = new List<int> { 1 },
        });
        // One more upcoming körlektion for Alice (two weeks out) so "Visa fler" shows on future
        _events.Add(new CalendarEvent
        {
            Id           = id++,
            TeacherId    = 2,
            StartTime    = today.AddDays(12).AddHours(14),
            EndTime      = today.AddDays(12).AddHours(15),
            IsBooked     = true,
            StudentName  = alice,
            LessonTypeId = 2,
            ResourceIds  = new List<int> { 2 },
        });

        // ── Past körlektion events for all other students (Utbildning-demo) ────
        // All students share the same EduPlan GlobalIds (same plan structure).
        var momentPool = new (string Gid, string Title)[]
        {
            ("51db5b27-4882-44cf-b2da-67fa258e2928",   "Information"),
            ("8aefff06-cc2c-4e7d-99e2-ab409092e888_0", "1 a klädsel och körstállning"),
            ("8aefff06-cc2c-4e7d-99e2-ab409092e888_1", "1 b reglage och instrument"),
            ("c01c2716-337b-47a3-abe3-ff2eb0c69ba3",   "2 a start och stannande"),
            ("b8e367d4-bb4c-4ea0-9f97-3fc732dc8625",   "2 b krypkörning och styrning"),
            ("e704aaa6-34fd-4428-8100-99ef302364b1_0", "3 a uppväxling"),
            ("e704aaa6-34fd-4428-8100-99ef302364b1_1", "3 b bromsning"),
            ("e704aaa6-34fd-4428-8100-99ef302364b1_2", "3 c nedväxling"),
            ("0fbe8f7a-73a4-459f-8848-4510b55807cc_0", "4 a motlut"),
            ("0fbe8f7a-73a4-459f-8848-4510b55807cc_1", "4 b medlut"),
            ("7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_0", "5 a låg fart"),
            ("7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_1", "5 b hög fart"),
            ("7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_2", "5 c acceleration"),
            ("7cf5d1af-df56-45af-8cc0-5a8bfdd9d316_3", "5 d hård bromsning"),
            ("c3bcc539-c96a-4221-b3cd-f27c2926be62",   "6 a mopeden"),
            ("70e51338-26b8-4863-b29b-f205ec886964_0", "6 b passagerare och last"),
            ("70e51338-26b8-4863-b29b-f205ec886964_1", "6 c släp"),
            ("1f8898b5-9d72-46e7-b2d5-adb4ca8772e4",   "6 d säkerhetskontroll"),
            ("4ad52141-5874-4151-bc1a-d0e02283245c",   "Människa"),
            ("95bd2655-7477-4b09-986b-0e01c7e6b698",   "Miljön"),
            ("d8173a97-1527-4dc5-b77f-8052a3904dc1_0", "7 a avsökning och riskbedömning"),
            ("d8173a97-1527-4dc5-b77f-8052a3904dc1_1", "7 b samordning och motorik"),
            ("a8150b06-2f30-4689-95e4-bf4c58c6cdff_0", "8 a avsökning och riskbedömning"),
            ("a8150b06-2f30-4689-95e4-bf4c58c6cdff_1", "8 b hastighetsanpassning"),
            ("a8150b06-2f30-4689-95e4-bf4c58c6cdff_2", "8 c placering"),
            ("f1c25bc5-3e7a-4c2a-9804-79137f67f235",   "8 d väjningsregler"),
            ("320f45ed-0e4e-4da7-80cb-f9bb25386fa4_0", "9 a avsökning och riskbedömning"),
            ("320f45ed-0e4e-4da7-80cb-f9bb25386fa4_1", "9 b hastighetsanpassning"),
            ("320f45ed-0e4e-4da7-80cb-f9bb25386fa4_2", "9 c placering"),
            ("ced62f5f-4df5-4e11-a5e5-7e5999c9bbee",   "9 d väjningsregler"),
            ("efe54055-f75e-4b6a-b55e-e4887d5cf837",   "9 e järnvägskorsning"),
            ("21b48419-26bc-42a9-bc79-d09e8faf2e58_0", "10 a avsökning och riskbedömning"),
            ("21b48419-26bc-42a9-bc79-d09e8faf2e58_1", "10 b hastighetsanpassning"),
            ("21b48419-26bc-42a9-bc79-d09e8faf2e58_2", "10 c placering"),
            ("8e5a7524-a963-4663-84b1-fa3bd2e490a9",   "10 d väjningsregler"),
            ("8c052992-1427-4792-83ce-f353efc2296c",   "10 e trafiksignal"),
            ("0ba163f6-11bf-4d39-ae18-14e5405d6299",   "10 f enkelriktad gata"),
            ("2a0cdac6-632d-4722-b980-35d3c37c233b",   "10 g cirkulationsplats"),
            ("7c482a48-f6d1-4964-b737-86459ae1ef22",   "10 h vändning och parkering"),
            ("226843f0-8297-4bb2-8b66-64ae7c891a64_0", "11 a avsökning och riskbedömning"),
            ("226843f0-8297-4bb2-8b66-64ae7c891a64_1", "11 b hastighetsanpassning"),
            ("226843f0-8297-4bb2-8b66-64ae7c891a64_2", "11 c placering"),
            ("f6908d5c-d545-4bc7-9065-5a69e8a23250_0", "11 d påfart och avfart"),
            ("f6908d5c-d545-4bc7-9065-5a69e8a23250_1", "11 e omkörning"),
            ("f6908d5c-d545-4bc7-9065-5a69e8a23250_2", "11 f vändning och parkering"),
            ("e79459b0-b3a6-48e7-92f2-14e8f33c0be2_0", "12 a avsökning och riskbedömning"),
            ("e79459b0-b3a6-48e7-92f2-14e8f33c0be2_1", "12 b hastighetsanpassning"),
            ("e79459b0-b3a6-48e7-92f2-14e8f33c0be2_2", "12 c motorväg"),
            ("e79459b0-b3a6-48e7-92f2-14e8f33c0be2_3", "12 d motortrafikled"),
            ("e79459b0-b3a6-48e7-92f2-14e8f33c0be2_4", "12 e mitträckesväg"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_0", "13 a avsökning och riskbedömning"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_1", "13 b hastighetsanpassning"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_2", "13 c mörkerdemonstration"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_3", "13 d möte"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_4", "13 e omkörning"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_5", "13 f parkering"),
            ("30c1056f-caf9-4b08-9358-bcc44afffc8d_6", "13 g nedsatt sikt"),
            ("add12ee3-862e-489f-a278-3a5c4ea9eb44_0", "14 a olika typer av halka"),
            ("add12ee3-862e-489f-a278-3a5c4ea9eb44_1", "14 b utrustning och hjälpsystem"),
        };

        foreach (var stu in Students)
        {
            if (stu.Name == "Alice Bergström") continue; // Alice has explicit data above

            var sid         = stu.Id;
            var lessonCount = 2 + (sid % 4);                    // 2–5 past lessons per student
            var poolStart   = (sid * 3) % (momentPool.Length - 9); // each student starts at a different spot
            var teacherIdS  = 1 + (sid % 6);                    // spread across teachers 1–6
            var hourS       = 8 + (sid % 4);                    // lesson start 08:00–11:00
            var resourceIdS = 1 + (sid % 3);                    // cars 1–3

            for (int li = 0; li < lessonCount; li++)
            {
                var daysAgo = -((lessonCount - li) * 7 + (sid % 5) + 1);
                var moments = new List<LessonMoment>();
                for (int mi = 0; mi < 3; mi++)
                {
                    var idx   = (poolStart + li * 3 + mi) % momentPool.Length;
                    var score = Math.Clamp(li + 1 + (sid % 2 == 0 ? 1 : 0) - (mi == 2 ? 1 : 0), 0, 5);
                    var comment = score switch
                    {
                        1                              => "Behöver mer träning.",
                        2 when (sid + li) % 2 == 0    => "Förbättring på gång.",
                        3 when mi == 2                 => "Svårare moment, behöver repeteras.",
                        4 when li == 0                 => "Bra för en nybörjare!",
                        _                              => "",
                    };
                    moments.Add(new LessonMoment
                    {
                        GlobalId = momentPool[idx].Gid,
                        Title    = momentPool[idx].Title,
                        Score    = score,
                        Comment  = comment,
                    });
                }
                _events.Add(new CalendarEvent
                {
                    Id            = id++,
                    TeacherId     = teacherIdS,
                    StartTime     = today.AddDays(daysAgo).AddHours(hourS),
                    EndTime       = today.AddDays(daysAgo).AddHours(hourS + 1),
                    IsBooked      = true,
                    StudentName   = stu.Name,
                    LessonTypeId  = 2,
                    ResourceIds   = new List<int> { resourceIdS },
                    LinkedMoments = moments,
                });
            }

            // One upcoming lesson per student
            var upcoming = 1 + (sid % 10);
            _events.Add(new CalendarEvent
            {
                Id           = id++,
                TeacherId    = teacherIdS,
                StartTime    = today.AddDays(upcoming).AddHours(hourS),
                EndTime      = today.AddDays(upcoming).AddHours(hourS + 1),
                IsBooked     = true,
                StudentName  = stu.Name,
                LessonTypeId = 2,
                ResourceIds  = new List<int> { resourceIdS },
            });
        }

        _nextEventId = id;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public IEnumerable<CalendarEvent> GetEventsForRange(DateTime start, DateTime end, CalendarFilter filter)
    {
        var selectedTeachers = filter.SelectedTeacherIds.Count > 0
            ? filter.SelectedTeacherIds
            : Teachers.Where(t => t.IsSelected).Select(t => t.Id).ToList();

        var result = new List<CalendarEvent>();

        foreach (var teacherId in selectedTeachers)
        {
            bool hasAnyTemplate = ScheduleTemplates.Any(t => t.TeacherId == teacherId);

            if (hasAnyTemplate)
            {
                // Booked events from _events (real bookings always shown)
                var booked = _events.Where(e =>
                    e.TeacherId == teacherId &&
                    e.StartTime < end && e.EndTime > start &&
                    e.IsBooked && filter.ShowBooked &&
                    (filter.SelectedLessonTypeIds.Count == 0 || (e.LessonTypeId.HasValue && filter.SelectedLessonTypeIds.Contains(e.LessonTypeId.Value))) &&
                    (filter.SelectedResourceIds.Count  == 0 || filter.SelectedResourceIds.All(r => e.ResourceIds.Contains(r)))
                ).ToList();
                result.AddRange(booked);

                // Available slots generated week-by-week from whichever template is active that week
                if (filter.ShowAvailable)
                {
                    foreach (var slot in GenerateTemplateSlotsForTeacher(teacherId, start, end))
                    {
                        bool conflict = booked.Any(b => b.StartTime < slot.EndTime && b.EndTime > slot.StartTime);
                        if (!conflict &&
                            (filter.SelectedLessonTypeIds.Count == 0 || (slot.LessonTypeId.HasValue && filter.SelectedLessonTypeIds.Contains(slot.LessonTypeId.Value))))
                            result.Add(slot);
                    }
                }
            }
            else
            {
                // No template — fall back to hand-coded events
                result.AddRange(_events.Where(e =>
                    e.TeacherId == teacherId &&
                    e.StartTime < end && e.EndTime > start &&
                    (filter.ShowBooked    || !e.IsBooked) &&
                    (filter.ShowAvailable || e.IsBooked)  &&
                    (filter.SelectedLessonTypeIds.Count == 0 || (e.LessonTypeId.HasValue && filter.SelectedLessonTypeIds.Contains(e.LessonTypeId.Value))) &&
                    (filter.SelectedResourceIds.Count  == 0 || filter.SelectedResourceIds.All(r => e.ResourceIds.Contains(r)))
                ));
            }
        }

        return result.OrderBy(e => e.StartTime).ToList();
    }

    // ── Template-driven slot generation ───────────────────────────────────────

    /// Returns the active template for a teacher on the given date.
    /// If multiple match (shouldn't happen if non-overlapping), latest StartDate wins.
    public ScheduleTemplate? GetActiveTemplate(int teacherId, DateOnly date)
        => ScheduleTemplates
            .Where(t => t.TeacherId == teacherId
                && (t.StartDate == null || t.StartDate <= date)
                && (t.EndDate   == null || t.EndDate   >= date))
            .OrderByDescending(t => t.StartDate ?? DateOnly.MinValue)
            .FirstOrDefault();

    /// Determines which week of the cycle a given Monday belongs to (1-based).
    private static int GetCycleWeek(ScheduleTemplate tmpl, DateOnly weekMonday)
    {
        var anchor       = tmpl.StartDate ?? new DateOnly(2024, 1, 1);
        var anchorMonday = anchor.AddDays(-(((int)anchor.DayOfWeek + 6) % 7));
        var weeksDiff    = (weekMonday.DayNumber - anchorMonday.DayNumber) / 7;
        return (weeksDiff % tmpl.CycleWeeks + tmpl.CycleWeeks) % tmpl.CycleWeeks + 1;
    }

    private int _ephemeralId = -1;

    /// Generates available slots week-by-week, picking the right template for each week.
    private IEnumerable<CalendarEvent> GenerateTemplateSlotsForTeacher(int teacherId, DateTime rangeStart, DateTime rangeEnd)
    {
        var slots  = new List<CalendarEvent>();
        var monday = rangeStart.Date.AddDays(-(((int)rangeStart.DayOfWeek + 6) % 7));

        while (monday < rangeEnd)
        {
            var weekMonday = DateOnly.FromDateTime(monday);
            var tmpl       = GetActiveTemplate(teacherId, weekMonday);
            if (tmpl != null)
            {
                var cycleWeek = GetCycleWeek(tmpl, weekMonday);
                foreach (var block in tmpl.TimeBlocks.Where(b => b.WeekNumber == cycleWeek))
                {
                    var blockDate     = monday.AddDays(block.DayOfWeek - 1);
                    var blockDateOnly = DateOnly.FromDateTime(blockDate);
                    if (blockDate < rangeStart || blockDate >= rangeEnd)           continue;
                    if (tmpl.StartDate != null && blockDateOnly < tmpl.StartDate)  continue;
                    if (tmpl.EndDate   != null && blockDateOnly > tmpl.EndDate)    continue;

                    var lt        = GetLessonType(block.LessonTypeId);
                    var slotStart = blockDate.Add(block.StartTime.ToTimeSpan());
                    var slotEnd   = slotStart.AddMinutes(lt?.DefaultDurationMinutes ?? 60);

                    slots.Add(new CalendarEvent
                    {
                        Id           = _ephemeralId--,
                        TeacherId    = teacherId,
                        StartTime    = slotStart,
                        EndTime      = slotEnd,
                        IsBooked     = false,
                        LessonTypeId = block.LessonTypeId,
                    });
                }
            }
            monday = monday.AddDays(7);
        }
        return slots;
    }

    public IEnumerable<CalendarEvent> GetEventsForDay(DateTime date, CalendarFilter filter)
        => GetEventsForRange(date.Date, date.Date.AddDays(1), filter);

    /// Returns the first free (unbooked) slot for a teacher after the given point in time.
    public CalendarEvent? GetNextAvailableSlot(int teacherId, DateTime after)
        => _events
            .Where(e => e.TeacherId == teacherId && !e.IsBooked && e.StartTime > after)
            .OrderBy(e => e.StartTime)
            .FirstOrDefault();

    /// <summary>All booked events (past + future) for a given student name.</summary>
    public IEnumerable<CalendarEvent> GetEventsForStudent(string studentName)
        => _events
            .Where(e => e.IsBooked &&
                (e.StudentName.Equals(studentName, StringComparison.OrdinalIgnoreCase) ||
                 e.ExtraStudents.Any(s => s.Equals(studentName, StringComparison.OrdinalIgnoreCase))))
            .OrderByDescending(e => e.StartTime)
            .ToList();

    public Teacher? GetTeacher(int id)
        => Teachers.FirstOrDefault(t => t.Id == id);

    public LessonType? GetLessonType(int id)
        => LessonTypes.FirstOrDefault(lt => lt.Id == id);

    public List<Resource> GetResourcesById(IEnumerable<int> ids)
        => Resources.Where(r => ids.Contains(r.Id)).ToList();

    // ── Mutations ─────────────────────────────────────────────────────────────

    public PickupLocation? GetPickupLocation(int id)
        => PickupLocations.FirstOrDefault(p => p.Id == id);

    public CalendarEvent AddBooking(BookingRequest req)
    {
        var evt = new CalendarEvent
        {
            Id               = _nextEventId++,
            TeacherId        = req.TeacherId,
            StartTime        = req.StartTime,
            EndTime          = req.EndTime,
            IsBooked         = true,
            StudentName      = req.StudentName,
            ExtraStudents    = req.ExtraStudents.ToList(),
            LessonTypeId     = req.LessonTypeId,
            ResourceIds      = req.SelectedResourceIds.ToList(),
            Notes            = req.Notes,
            PickupLocationId = req.PickupLocationId,
        };
        _events.Add(evt);
        return evt;
    }

    public CalendarEvent AddAvailableSlot(int teacherId, DateTime start, DateTime end)
    {
        var evt = new CalendarEvent
        {
            Id        = _nextEventId++,
            TeacherId = teacherId,
            StartTime = start,
            EndTime   = end,
            IsBooked  = false,
        };
        _events.Add(evt);
        return evt;
    }

    public void CancelBooking(int eventId)
    {
        var evt = _events.FirstOrDefault(e => e.Id == eventId);
        if (evt is not null)
        {
            // Turn back into an available slot
            evt.IsBooked      = false;
            evt.StudentName   = "";
            evt.ExtraStudents = new();
            evt.LessonTypeId  = null;
            evt.ResourceIds   = new();
            evt.Notes         = "";
        }
    }

    public void DeleteEvent(int eventId)
        => _events.RemoveAll(e => e.Id == eventId);

    // ── Lesson type CRUD ──────────────────────────────────────────────────────

    public void AddOrUpdateLessonType(LessonType lt)
    {
        var existing = LessonTypes.FirstOrDefault(l => l.Id == lt.Id);
        if (existing is not null)
        {
            existing.Name                   = lt.Name;
            existing.Description            = lt.Description;
            existing.DefaultDurationMinutes = lt.DefaultDurationMinutes;
            existing.MaxStudents            = lt.MaxStudents;
            existing.Color                  = lt.Color;
            existing.LightColor             = lt.LightColor;
            existing.IsBookable             = lt.IsBookable;
            existing.RequiresIdCheck        = lt.RequiresIdCheck;
        }
        else
        {
            lt.Id = LessonTypes.Count > 0 ? LessonTypes.Max(l => l.Id) + 1 : 1;
            LessonTypes.Add(lt);
        }
    }

    public void DeleteLessonType(int id)
        => LessonTypes.RemoveAll(l => l.Id == id);

    // ── Schedule templates ────────────────────────────────────────────────────

    private int _nextBlockId = 100;
    public int GetNextBlockId() => _nextBlockId++;

    public List<ScheduleTemplate> ScheduleTemplates { get; } = new()
    {
        new()
        {
            Id = 1, Name = "Standard vecka", CycleWeeks = 1, TeacherId = 1,
            StartDate = new DateOnly(2026, 1, 5), // Gäller fr.o.m. v.2 2026
            TimeBlocks = new List<ScheduleTimeBlock>
            {
                // Måndag
                new() { Id = 1, WeekNumber = 1, DayOfWeek = 1, StartTime = new TimeOnly(8, 0),  LessonTypeId = 2 },
                new() { Id = 2, WeekNumber = 1, DayOfWeek = 1, StartTime = new TimeOnly(9, 0),  LessonTypeId = 2 },
                new() { Id = 3, WeekNumber = 1, DayOfWeek = 1, StartTime = new TimeOnly(10, 0), LessonTypeId = 2 },
                new() { Id = 4, WeekNumber = 1, DayOfWeek = 1, StartTime = new TimeOnly(12, 0), LessonTypeId = 9 },
                new() { Id = 5, WeekNumber = 1, DayOfWeek = 1, StartTime = new TimeOnly(13, 0), LessonTypeId = 2 },
                new() { Id = 6, WeekNumber = 1, DayOfWeek = 1, StartTime = new TimeOnly(14, 0), LessonTypeId = 2 },
                // Tisdag
                new() { Id = 7,  WeekNumber = 1, DayOfWeek = 2, StartTime = new TimeOnly(8, 0),  LessonTypeId = 2 },
                new() { Id = 8,  WeekNumber = 1, DayOfWeek = 2, StartTime = new TimeOnly(9, 0),  LessonTypeId = 2 },
                new() { Id = 9,  WeekNumber = 1, DayOfWeek = 2, StartTime = new TimeOnly(10, 0), LessonTypeId = 4 },
                new() { Id = 10, WeekNumber = 1, DayOfWeek = 2, StartTime = new TimeOnly(12, 0), LessonTypeId = 9 },
                new() { Id = 11, WeekNumber = 1, DayOfWeek = 2, StartTime = new TimeOnly(13, 0), LessonTypeId = 2 },
                // Onsdag
                new() { Id = 12, WeekNumber = 1, DayOfWeek = 3, StartTime = new TimeOnly(8, 0),  LessonTypeId = 2 },
                new() { Id = 13, WeekNumber = 1, DayOfWeek = 3, StartTime = new TimeOnly(9, 0),  LessonTypeId = 2 },
                new() { Id = 14, WeekNumber = 1, DayOfWeek = 3, StartTime = new TimeOnly(12, 0), LessonTypeId = 9 },
                new() { Id = 15, WeekNumber = 1, DayOfWeek = 3, StartTime = new TimeOnly(13, 0), LessonTypeId = 2 },
                new() { Id = 16, WeekNumber = 1, DayOfWeek = 3, StartTime = new TimeOnly(14, 0), LessonTypeId = 2 },
                // Torsdag
                new() { Id = 17, WeekNumber = 1, DayOfWeek = 4, StartTime = new TimeOnly(8, 0),  LessonTypeId = 2 },
                new() { Id = 18, WeekNumber = 1, DayOfWeek = 4, StartTime = new TimeOnly(9, 0),  LessonTypeId = 2 },
                new() { Id = 19, WeekNumber = 1, DayOfWeek = 4, StartTime = new TimeOnly(10, 0), LessonTypeId = 2 },
                new() { Id = 20, WeekNumber = 1, DayOfWeek = 4, StartTime = new TimeOnly(12, 0), LessonTypeId = 9 },
                new() { Id = 21, WeekNumber = 1, DayOfWeek = 4, StartTime = new TimeOnly(13, 0), LessonTypeId = 2 },
                // Fredag
                new() { Id = 22, WeekNumber = 1, DayOfWeek = 5, StartTime = new TimeOnly(8, 0),  LessonTypeId = 2 },
                new() { Id = 23, WeekNumber = 1, DayOfWeek = 5, StartTime = new TimeOnly(9, 0),  LessonTypeId = 2 },
                new() { Id = 24, WeekNumber = 1, DayOfWeek = 5, StartTime = new TimeOnly(12, 0), LessonTypeId = 9 },
                new() { Id = 25, WeekNumber = 1, DayOfWeek = 5, StartTime = new TimeOnly(13, 0), LessonTypeId = 2 },
            }
        }
    };

    /// Returns all templates for a teacher sorted by start date.
    public List<ScheduleTemplate> GetTemplatesForTeacher(int teacherId)
        => ScheduleTemplates
            .Where(t => t.TeacherId == teacherId)
            .OrderBy(t => t.StartDate ?? DateOnly.MaxValue)
            .ToList();

    /// Returns true if the candidate's date range overlaps any other template for the same teacher.
    public bool HasTemplateOverlap(ScheduleTemplate candidate)
    {
        var cStart = candidate.StartDate ?? DateOnly.MinValue;
        var cEnd   = candidate.EndDate   ?? DateOnly.MaxValue;
        return ScheduleTemplates
            .Where(t => t.TeacherId == candidate.TeacherId && t.Id != candidate.Id)
            .Any(t =>
            {
                var tStart = t.StartDate ?? DateOnly.MinValue;
                var tEnd   = t.EndDate   ?? DateOnly.MaxValue;
                return cStart <= tEnd && cEnd >= tStart;
            });
    }

    /// Returns null on success, or an error message if the date range overlaps another template.
    public string? SaveScheduleTemplate(ScheduleTemplate template)
    {
        if (HasTemplateOverlap(template))
            return "Datumintervallet överlappar med ett annat schema för denna lärare.";

        var existing = ScheduleTemplates.FirstOrDefault(t => t.Id == template.Id);
        if (existing is not null)
            ScheduleTemplates.Remove(existing);
        if (template.Id == 0)
            template.Id = ScheduleTemplates.Count > 0 ? ScheduleTemplates.Max(t => t.Id) + 1 : 1;
        ScheduleTemplates.Add(template);
        return null;
    }
}
