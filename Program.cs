using BookingDemo.Models;
using BookingDemo.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Session-based simple auth
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name        = "booking_sess";
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddSingleton<BookingService>();

// Tools delas mellan TestLlmReportAgent och OpenAIReportAgent.
builder.Services.AddSingleton<IReadOnlyList<AgentTool>>(sp =>
    ReportTools.Create(sp.GetRequiredService<BookingService>()));

// Auto-välj agent: OpenAI om nyckel finns, annars deterministisk TestLlm.
// Ger samma användarupplevelse i båda fallen – samma tools, samma rapporter.
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrWhiteSpace(openAiKey))
    openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (!string.IsNullOrWhiteSpace(openAiKey))
{
    var model = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini";
    Console.WriteLine($"[ReportAgent] Using OpenAIReportAgent (model: {model})");
    builder.Services.AddScoped<IReportAgent>(sp =>
        new OpenAIReportAgent(openAiKey, sp.GetRequiredService<IReadOnlyList<AgentTool>>(), model));
}
else
{
    Console.WriteLine("[ReportAgent] Using TestLlmReportAgent (no OpenAI key configured)");
    builder.Services.AddScoped<IReportAgent>(sp =>
        new TestLlmReportAgent(sp.GetRequiredService<IReadOnlyList<AgentTool>>()));
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// ── Simple password gate ───────────────────────────────────────────────────
// Bypass for: /login, static assets, Blazor hub & framework files
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";

    bool bypass =
        path.StartsWith("/login",       StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_blazor",     StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_framework",  StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_content",    StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/css/",        StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/js/",         StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/favicon.ico",     StringComparison.OrdinalIgnoreCase);

    if (!bypass && context.Session.GetString("authenticated") != "true")
    {
        context.Response.Redirect("/login");
        return;
    }

    await next(context);
});

app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

// Railway (and most cloud platforms) inject a PORT env variable.
// Fall back to 5000 for local development.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
