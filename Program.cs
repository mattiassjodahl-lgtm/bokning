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
