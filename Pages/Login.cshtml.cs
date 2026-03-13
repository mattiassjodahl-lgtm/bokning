using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingDemo.Pages;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;

    public bool InvalidPassword { get; private set; }

    public LoginModel(IConfiguration config) => _config = config;

    public void OnGet() { }

    public IActionResult OnPost(string password)
    {
        var correct = _config["DemoPassword"] ?? "körskola123";

        if (password == correct)
        {
            HttpContext.Session.SetString("authenticated", "true");
            return Redirect("/");
        }

        InvalidPassword = true;
        return Page();
    }
}
