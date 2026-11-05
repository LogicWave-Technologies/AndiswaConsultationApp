using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AndiswaConsultationApp.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _api;   // our link to the API (given to us by MauiProgram.cs)
    private bool _loaded;               // true once we got data from the SERVER (not just the saved copy)

    public HomePage(ApiService api)
    {
        InitializeComponent();
        _api = api;

        // The dots under the reviews follow the carousel
        ReviewsCarousel.IndicatorView = ReviewDots;

        // What happens when the user pulls the page down
        Refresh.Command = new Command(async () =>
        {
            await LoadAsync(forceRefresh: true);
            Refresh.IsRefreshing = false;
        });

        // What happens when the user taps "Retry" in the red banner
        ErrorBox.Retry += async (_, _) => await LoadAsync(forceRefresh: true);
    }

    // Runs every time the page comes on screen
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowGreetingAsync();          // the name may have changed (logged in / out)
        if (!_loaded)
            await LoadAsync(forceRefresh: false);
    }

    // "Good morning, Ayanda" - an app greets YOU, a website greets everybody the same
    private async Task ShowGreetingAsync()
    {
        await ClientSession.LoadAsync();

        var hour = DateTime.Now.Hour;
        var part = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";

        var words = ClientSession.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (ClientSession.IsLoggedIn && words.Length > 0)
        {
            var first = words[0];
            GreetingLabel.Text = $"{part} 👋";
            NameLabel.Text = first;

            InitialsLabel.Text = string.Concat(words.Take(2).Select(p => char.ToUpper(p[0])));
            InitialsLabel.IsVisible = true;
            AccountIcon.IsVisible = false;
        }
        else
        {
            GreetingLabel.Text = $"{part} 👋";
            NameLabel.Text = "What shall we create?";
            InitialsLabel.IsVisible = false;
            AccountIcon.IsVisible = true;
        }
    }

    private async Task LoadAsync(bool forceRefresh)
    {
        ErrorBox.Hide();

        try
        {
            // Step 1: show the saved copy straight away, so the page is never empty
            if (BindingContext is null)
                Show(await _api.GetSavedStudioAsync());

            // Step 2: ask the server for the latest information
            var studio = await _api.GetStudioAsync(forceRefresh);
            Show(studio);

            // Step 3: if we are offline, tell the user and try the server again next time
            _loaded = !_api.UsingOfflineData;
            if (_api.UsingOfflineData)
                ErrorBox.ShowInfo("You're offline. Showing the information saved on your phone.");
        }
        catch (Exception ex)
        {
            ErrorBox.Show(ApiService.FriendlyError(ex));
        }
    }

    private void Show(StudioInfo studio)
    {
        BindingContext = studio;
        BuildStats(studio.Stats);
    }

    // The small number chips ("50+ Projects completed"), built in code because they come from the database.
    // ".Themed(...)" ties a colour to the theme so it follows dark / light mode.
    private void BuildStats(List<StatItem> stats)
    {
        StatsLayout.Children.Clear();

        foreach (var stat in stats)
        {
            var chip = new Border
            {
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Padding = new Thickness(16, 10),
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children =
                    {
                        new Label { Text = stat.Value, FontSize = 20 }
                            .Themed(Label.TextColorProperty, "Primary")
                            .Themed(Label.FontFamilyProperty, "HeadingFont"),
                        new Label { Text = stat.Label, FontSize = 11 }
                            .Themed(Label.TextColorProperty, "Muted")
                    }
                }
            }
            .Themed(Border.StrokeProperty, "Line")
            .Themed(VisualElement.BackgroundColorProperty, "Surface");

            StatsLayout.Children.Add(chip);
        }
    }

    // ---- taps ----

    private async void OnQuoteClicked(object? sender, EventArgs e)
    {
        Haptics.Tap();
        await AppNavigation.GoToTabAsync("Contact");
    }

    private async void OnAccountTapped(object? sender, TappedEventArgs e)
    {
        Haptics.Tap();
        await AppNavigation.GoToAccountAsync();
    }

    private async void OnCallTapped(object? sender, TappedEventArgs e) => await QuickContact.CallAsync();
    private async void OnWhatsAppTapped(object? sender, TappedEventArgs e) => await QuickContact.WhatsAppAsync();
    private async void OnEmailTapped(object? sender, TappedEventArgs e) => await QuickContact.EmailAsync();
    private async void OnDirectionsTapped(object? sender, TappedEventArgs e) => await QuickContact.DirectionsAsync();

    // A service card in the swipe row: start a quote for that service
    private async void OnServiceTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject card && card.BindingContext is ServiceItem service)
        {
            Haptics.Tap();
            await AppNavigation.RequestServiceAsync(service.ContactService);
        }
    }

    // A project in the swipe row: open its detail page
    private async void OnProjectTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject card && card.BindingContext is ProjectItem project)
            await AppNavigation.OpenProjectAsync(project);
    }

    private async void OnServicesTapped(object? sender, TappedEventArgs e) =>
        await AppNavigation.GoToTabAsync("Services");

    private async void OnWorkTapped(object? sender, TappedEventArgs e) =>
        await AppNavigation.GoToTabAsync("Portfolio");

    private async void OnTestimonialsTapped(object? sender, TappedEventArgs e) =>
        await AppNavigation.OpenPageAsync("testimonials");

    private async void OnAboutTapped(object? sender, TappedEventArgs e) =>
        await AppNavigation.OpenPageAsync("about");
}
