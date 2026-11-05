using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;

namespace AndiswaConsultationApp.Pages;

public partial class ClientDashboardPage : ContentPage
{
    private readonly ApiService _api;   // our link to the API
    private readonly Controls.ErrorBox _errorBox;

    public ClientDashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        _errorBox = (Controls.ErrorBox)FindByName("ErrorBox");

        _errorBox.Retry += async (_, _) => await LoadAsync();   // "Try again" button
    }

    // Runs every time the dashboard comes on screen, so the status is always up to date
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Not logged in? This page needs a login, so go to the login page.
        await ClientSession.LoadAsync();
        if (!ClientSession.IsLoggedIn)
        {
            // Dispatch = "do it right after this page has finished appearing"
            Dispatcher.Dispatch(async () => await AppNavigation.GoToTabAsync(AppNavigation.ClientLoginRoute));
            return;
        }

        // Show the saved name straight away, then load the latest data
        ShowName(ClientSession.Name);
        await LoadAsync();
    }

    // Pull down to refresh
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync(showSpinner: false);
        Refresh.IsRefreshing = false;
    }

    // Asks the API for this client's quote requests and projects
    private async Task LoadAsync(bool showSpinner = true)
    {
        if (ClientSession.Token is not { } token)
            return;

        _errorBox.Hide();
        if (showSpinner)
            SetBusy(true);

        try
        {
            var dashboard = await _api.GetDashboardAsync(token);
            Show(dashboard);
        }
        catch (UnauthorizedAccessException)
        {
            // The login expired or the account was removed: log out and ask to log in again
            ClientSession.Clear();
            await DisplayAlertAsync("Please log in again", "Your login has expired. Please log in again.", "OK");
            await AppNavigation.GoToTabAsync(AppNavigation.ClientLoginRoute);
        }
        catch (Exception ex)
        {
            _errorBox.Show(ApiService.FriendlyError(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    // Puts the data on the screen
    private void Show(ClientDashboard dashboard)
    {
        ShowName(dashboard.Name);
        QuoteCount.Text = dashboard.Quotes.Count.ToString();
        ProjectCount.Text = dashboard.Projects.Count.ToString();

        BindableLayout.SetItemsSource(QuotesList, dashboard.Quotes);
        NoQuotesLabel.IsVisible = dashboard.Quotes.Count == 0;

        BindableLayout.SetItemsSource(ProjectsList, dashboard.Projects);
        NoProjectsLabel.IsVisible = dashboard.Projects.Count == 0;
    }

    // "Welcome, Ayanda" + the initials in the circle
    private void ShowName(string name)
    {
        WelcomeLabel.Text = name.Length > 0 ? name : "Welcome";
        InitialsLabel.Text = string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                               .Take(2).Select(p => char.ToUpper(p[0])));
    }

    // "+ New quote request": open the Quote screen
    private async void OnNewQuoteClicked(object? sender, EventArgs e)
    {
        Haptics.Tap();
        await AppNavigation.GoToTabAsync("Contact");
    }

    // "Log out" button in the header
    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var sure = await DisplayAlertAsync("Log out", "Do you want to log out of the client portal?", "Log out", "Cancel");
        if (!sure)
            return;

        ClientSession.Clear();

        // Empty the lists so the next person never sees this client's data
        BindableLayout.SetItemsSource(QuotesList, null);
        BindableLayout.SetItemsSource(ProjectsList, null);
        ShowName("");
        QuoteCount.Text = ProjectCount.Text = "0";

        await AppNavigation.GoToTabAsync(AppNavigation.ClientLoginRoute);
    }

    private void SetBusy(bool busy)
    {
        Busy.IsRunning = busy;
        Busy.IsVisible = busy;
    }
}
