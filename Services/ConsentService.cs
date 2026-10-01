namespace BookingDemo.Services;

/// <summary>
/// Delar tillståndet för cookiebannern mellan sidfoten ("Cookieinställningar") och
/// Tracking-komponenten. En instans per besökare (scoped).
/// </summary>
public class ConsentService
{
    public bool BannerOpen { get; private set; }
    public bool FocusBanner { get; private set; }

    public event Action? Changed;

    /// <param name="focus">True när besökaren själv öppnat bannern, så att fokus flyttas dit.</param>
    public void Open(bool focus)
    {
        BannerOpen = true;
        FocusBanner = focus;
        Changed?.Invoke();
    }

    public void Close()
    {
        BannerOpen = false;
        FocusBanner = false;
        Changed?.Invoke();
    }
}
