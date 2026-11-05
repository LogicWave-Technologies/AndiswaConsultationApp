
using AndiswaConsultationApp.Models;

namespace AndiswaConsultationApp.Services;

public static class AppNavigation
{
    // The three Account pages (login, register, dashboard). They are tabs too (see AppShell.xaml).
    // They open from the account circle on Home and the account card on More (the bottom bar marks "More").
    public const string ClientLoginRoute = "ClientLogin";
    public const string ClientDashboardRoute = "ClientDashboard";
    public const string ClientRegisterRoute = "ClientRegister";

    // Switch to one of the tabs: "Home", "Services", "Portfolio" (Work), "Contact" (Quote) or "More"
    public static Task GoToTabAsync(string tab) =>
        Shell.Current.GoToAsync($"//{tab}");

    // After a good login or registration (or when the app starts and you are still logged in): open Home
    public static Task GoToHomeAsync() => GoToTabAsync("Home");

    // The account circle / card: the dashboard if the client is logged in, otherwise the login page
    public static async Task GoToAccountAsync()
    {
        await ClientSession.LoadAsync();   // reads the saved login the first time
        await GoToTabAsync(ClientSession.IsLoggedIn ? ClientDashboardRoute : ClientLoginRoute);
    }

    // Open a page that is not a tab: "about", "team" or "testimonials"
    public static Task OpenPageAsync(string route) =>
        Shell.Current.GoToAsync(route);

    // Open the detail page of one project (it receives the project through the "Project" key)
    public static Task OpenProjectAsync(ProjectItem project) =>
        Shell.Current.GoToAsync("projectdetail", new Dictionary<string, object> { ["Project"] = project });

    // Go to the Contact tab with a service already chosen in the drop-down
    public static Task RequestServiceAsync(string? contactService)
    {
        AppState.PendingService = contactService;
        return GoToTabAsync("Contact");
    }

    // Go back one page (the same as the back arrow)
    public static Task BackAsync() =>
        Shell.Current.GoToAsync("..");
}
