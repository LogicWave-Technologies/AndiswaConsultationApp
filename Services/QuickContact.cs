// ============================================================================
// QuickContact.cs  -  "Call", "WhatsApp", "Email" and "Directions" buttons

// ============================================================================
using AndiswaConsultationApp.Models;

namespace AndiswaConsultationApp.Services;

public static class QuickContact
{
    private static string Phone => Pick(AppState.Studio?.Phone, SiteContent.Phone);
    private static string Email => Pick(AppState.Studio?.Email, SiteContent.Email);
    private static string Address => Pick(AppState.Studio?.Address, SiteContent.Address);

    public static Task CallAsync()
    {
        Haptics.Tap();
        return OpenAsync($"tel:{Digits(Phone, keepPlus: true)}");
    }

    // wa.me needs the number with the country code and no "+" or spaces: 27677904428
    public static Task WhatsAppAsync(string message = "Hi Andiswa, I'd like to find out more about your design services.")
    {
        Haptics.Tap();
        return OpenAsync($"https://wa.me/{Digits(Phone, keepPlus: false)}?text={Uri.EscapeDataString(message)}");
    }

    public static Task EmailAsync()
    {
        Haptics.Tap();
        return OpenAsync($"mailto:{Email}");
    }

    // Opens Google Maps (the app if it is installed, otherwise the browser)
    public static Task DirectionsAsync()
    {
        Haptics.Tap();
        return OpenAsync($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(Address)}");
    }

    private static async Task OpenAsync(string uri)
    {
        try
        {
            await Launcher.Default.OpenAsync(uri);
        }
        catch
        {
            // No app can open it (e.g. no dialler on a tablet / PC)
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Not available", "This device cannot open that. You can reach us at " + Phone + ".", "OK");
        }
    }

    private static string Pick(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    // "+27 67 790 4428" -> "+27677904428" (tel:) or "27677904428" (WhatsApp)
    private static string Digits(string phone, bool keepPlus)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith('0'))
            digits = "27" + digits[1..];        // a local number like 067... becomes 2767...
        return keepPlus ? "+" + digits : digits;
    }
}
