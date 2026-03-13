# Bokningssystem Demo – MudBlazor / Blazor Server

En interaktiv kalenderapplikation för körskolor byggd med **MudBlazor** och **.NET 8 Blazor Server**.

## Funktioner

| Funktion | Beskrivning |
|---|---|
| **Dag / Vecka / Månad** | Byt kalendervy via knappar i appbaren |
| **Flera lärare** | Se alla lärares scheman parallellt, färgkodade |
| **Boka lektion** | Klicka på ledig tid (grön) eller `+`-knappen |
| **Lektionstyp 1–7** | Välj bland 7 lektionstyper vid bokning |
| **Resurser** | Knyt bil, klassrum eller simulator till bokningen |
| **Avboka** | Klicka på bokad lektion → Avboka |
| **Filterdrager** | Filtrera på lärare, visa bara lediga/bokade, välj lektionstyp |
| **Mobilläge** | Veckovy scrollar horisontellt, alla lärare visas |

## Snabbstart

```bash
# Kräver .NET 8 SDK (https://dot.net)
cd BookingDemo
dotnet run
```

Öppna sedan **http://localhost:5000** i webbläsaren.

## Projektstruktur

```
BookingDemo/
├── Models/
│   └── Models.cs               # Teacher, LessonType, Resource, CalendarEvent, ...
├── Services/
│   └── BookingService.cs       # In-memory service med mock-data (6 lärare, veckoscheman)
├── Components/
│   ├── WeekView.razor          # Dag- och veckokalender med absolut positionerade block
│   ├── MonthView.razor         # Månadsöversikt
│   └── BookingDialog.razor     # Boknings- / visningsdialog
├── Pages/
│   ├── Index.razor             # Huvudsida – navigation, filter, vybyte
│   └── _Host.cshtml            # Blazor Server host-sida
├── Shared/
│   └── MainLayout.razor        # MudBlazor-layout med tema
└── wwwroot/css/
    ├── app.css
    └── calendar.css            # Kalender-layout, event-blocks, responsivitet
```

## Mock-data (BookingService.cs)

- **6 lärare**: Anna, Erik, Maria, Johan, Petra, Lars – med unika färger
- **7 lektionstyper**: Introduktionslektion, Körlektion B, Motorvägslektion, m.m.
- **7 resurser**: 4 bilar, 2 klassrum, 1 simulator
- Veckoscheman med blandning av lediga tider och bokade lektioner

## Utbyggnad

För produktion rekommenderas:
- Byt `BookingService` (in-memory) mot Entity Framework Core med SQL Server/PostgreSQL
- Lägg till ASP.NET Core Identity för inloggning
- Implementera SignalR för realtidsuppdateringar när flera användare bokar samtidigt
