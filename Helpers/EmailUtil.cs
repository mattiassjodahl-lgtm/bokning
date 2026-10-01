using System.Net.Mail;

namespace BookingDemo.Helpers;

/// <summary>Kontroll av e-postadresser som administratören skriver in.</summary>
public static class EmailUtil
{
    /// <summary>
    /// En adress, utan namn eller flera mottagare, och med en punkt i domänen (MailAddress släpper igenom "a@b").
    /// </summary>
    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var value = email.Trim();
        if (!MailAddress.TryCreate(value, out var parsed) || parsed.Address != value) return false;
        var domain = parsed.Host;
        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    /// <summary>Felmeddelande för ett valfritt adressfält. Tomt fält är tillåtet.</summary>
    public static string? Error(string? email) =>
        string.IsNullOrWhiteSpace(email) || IsValid(email)
            ? null
            : "Ange en giltig e-postadress, t.ex. info@skolan.se.";
}
