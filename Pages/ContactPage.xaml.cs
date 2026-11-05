// ============================================================================
// ContactPage.xaml.cs  -  the 3-step QUOTE screen
// ----------------------------------------------------------------------------
//   _step = 1  pick a service tile
//   _step = 2  name + email (+ phone)
//   _step = 3  message + summary, then Send
// Each step is checked before you can continue, and the problem is shown in red
// under the step (no pop-up boxes). After sending, the "Request sent" panel shows.
// ============================================================================
using System.Net.Mail;
using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AndiswaConsultationApp.Pages;

public partial class ContactPage : ContentPage
{
    // The same choices as the website's contact form, so the admin sees familiar values.
    // The emoji is only for the tile in the app.
    private static readonly (string Name, string Icon, string Hint)[] Services =
    {
        ("Graphic Design", "🎨", "Flyers, posters, brochures"),
        ("Branding & Identity", "🗂️", "Logos, cards, brand kits"),
        ("Social Media Design", "📱", "Posts, stories, templates"),
        ("Web Design", "🖥️", "Websites and landing pages"),
        ("Business Consultation", "📘", "Plans, profiles, proposals"),
        ("Other", "✨", "Something else"),
    };

    private readonly ApiService _api;   // our link to the API
    private StudioInfo? _studio;        // the contact details currently shown

    private int _step = 1;
    private string _service = "";
    private readonly Dictionary<string, (Border Tile, Label Tick)> _tiles = new();

    public ContactPage(ApiService api)
    {
        InitializeComponent();
        _api = api;

        ErrorBox.Retry += async (_, _) => await LoadStudioAsync();     // "Retry" button
        BuildServiceTiles();
        ShowStep(1);
    }

    // Runs every time the Quote tab comes on screen
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ThemeService.Changed += RefreshColours;
        RefreshColours();

        // Did the user arrive by tapping a service somewhere else? Select it and go to step 2.
        if (AppState.PendingService is { } wanted)
        {
            AppState.PendingService = null;     // use it only once
            if (DonePanel.IsVisible)
                ResetForm();
            SelectService(Services.Any(s => s.Name == wanted) ? wanted : "Other");
            ShowStep(2);
        }

        // Logged in? Fill in the name and email for the client (an app knows who you are).
        await ClientSession.LoadAsync();
        if (ClientSession.IsLoggedIn)
            PrefillFromAccount();

        // Try to send any messages that were saved while the phone was offline
        _ = _api.SyncPendingAsync();

        // Load the details if we have none yet, or if last time we were offline
        if (_studio is null || _api.UsingOfflineData)
            await LoadStudioAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ThemeService.Changed -= RefreshColours;
    }

    // Loads the email, phone, address, hours and owner details
    private async Task LoadStudioAsync()
    {
        ErrorBox.Hide();
        try
        {
            if (_studio is null)
                Show(await _api.GetSavedStudioAsync());

            Show(await _api.GetStudioAsync());

            if (_api.UsingOfflineData)
                ErrorBox.ShowInfo("You're offline. You can still send a request: it goes out when you're back online.");
        }
        catch (Exception ex)
        {
            ErrorBox.Show(ApiService.FriendlyError(ex));
        }
    }

    private void Show(StudioInfo studio)
    {
        _studio = studio;
        BindingContext = studio;
    }

    private void PrefillFromAccount()
    {
        var parts = ClientSession.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) && parts.Length > 0)
            FirstNameEntry.Text = parts[0];
        if (string.IsNullOrWhiteSpace(LastNameEntry.Text) && parts.Length > 1)
            LastNameEntry.Text = parts[1];
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            EmailEntry.Text = ClientSession.Email;
    }

    // ===================== STEP 1: the service tiles =====================

    private void BuildServiceTiles()
    {
        for (var i = 0; i < Services.Length; i++)
        {
            var (name, icon, hint) = Services[i];

            if (i % 2 == 0)
                ServiceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            var tick = new Label
            {
                Text = "✓",
                FontSize = 14,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            }.Themed(Label.TextColorProperty, "Primary").Themed(Label.FontFamilyProperty, "HeadingFont");

            var tile = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                Padding = new Thickness(14),
                Content = new Grid
                {
                    Children =
                    {
                        new VerticalStackLayout
                        {
                            Spacing = 6,
                            Children =
                            {
                                new Label { Text = icon, FontSize = 26 },
                                new Label { Text = name, FontSize = 14, LineBreakMode = LineBreakMode.WordWrap }
                                    .Themed(Label.TextColorProperty, "Ink")
                                    .Themed(Label.FontFamilyProperty, "HeadingFont"),
                                new Label { Text = hint, FontSize = 11 }
                                    .Themed(Label.TextColorProperty, "Muted")
                            }
                        },
                        tick
                    }
                }
            };

            var chosen = name;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                Haptics.Tap();
                SelectService(chosen);
            };
            tile.GestureRecognizers.Add(tap);
            SemanticProperties.SetDescription(tile, name);

            _tiles[name] = (tile, tick);
            ServiceGrid.Add(tile, i % 2, i / 2);
        }

        RefreshColours();
    }

    private void SelectService(string name)
    {
        _service = name;
        StepError.IsVisible = false;
        RefreshColours();
    }

    // Colours of the tiles and the progress bars, read from the current theme (so dark mode works)
    private void RefreshColours()
    {
        foreach (var (name, (tile, tick)) in _tiles)
        {
            var selected = name == _service;
            tile.BackgroundColor = ThemeService.Get(selected ? "PrimarySoft" : "Surface");
            tile.Stroke = ThemeService.Get(selected ? "Primary" : "Line");
            tile.StrokeThickness = selected ? 2 : 1;
            tick.IsVisible = selected;
        }

        Bar1.Color = ThemeService.Get("Primary");
        Bar2.Color = ThemeService.Get(_step >= 2 ? "Primary" : "Line");
        Bar3.Color = ThemeService.Get(_step >= 3 ? "Primary" : "Line");
    }

    // ===================== moving between steps =====================

    private void ShowStep(int step)
    {
        _step = step;
        Step1.IsVisible = step == 1;
        Step2.IsVisible = step == 2;
        Step3.IsVisible = step == 3;
        DonePanel.IsVisible = false;
        StepError.IsVisible = false;
        Header.IsVisible = true;
        ActionBar.IsVisible = true;

        StepLabel.Text = step switch
        {
            1 => "Step 1 of 3 · Service",
            2 => "Step 2 of 3 · Your details",
            _ => "Step 3 of 3 · Your project"
        };

        BackButton.IsVisible = step > 1;
        NextButton.Text = step == 3 ? "Send request" : "Continue";
        NextButton.Style = (Style)Application.Current!.Resources[step == 3 ? "AccentButton" : "PrimaryButton"];

        if (step == 3)
            FillSummary();

        RefreshColours();
        ScrollToTop();
    }

    // Back to the top of the step (only once the page is on screen)
    private void ScrollToTop()
    {
        if (StepScroll.Handler is not null)
            _ = StepScroll.ScrollToAsync(0, 0, false);
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        if (_step > 1)
            ShowStep(_step - 1);
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        var problem = CheckStep(_step);
        if (problem is not null)
        {
            StepError.Text = problem;
            StepError.IsVisible = true;
            return;
        }

        Haptics.Tap();
        if (_step < 3)
            ShowStep(_step + 1);
        else
            await SendAsync();
    }

    // Returns what is wrong with a step, or null when it is fine
    private string? CheckStep(int step)
    {
        switch (step)
        {
            case 1:
                return _service.Length == 0 ? "Please tap the service you need." : null;

            case 2:
                if ((FirstNameEntry.Text?.Trim() ?? "").Length == 0)
                    return "Please enter your first name.";
                if (!IsValidEmail(EmailEntry.Text?.Trim() ?? ""))
                    return "Please enter a valid email address, e.g. you@company.co.za.";
                return null;

            default:
                return (MessageEditor.Text?.Trim() ?? "").Length == 0
                    ? "Please tell us a little about your project."
                    : null;
        }
    }

    private void FillSummary()
    {
        SummaryService.Text = _service;
        SummaryContact.Text = $"{FirstNameEntry.Text?.Trim()} {LastNameEntry.Text?.Trim()}".Trim();
        var phone = PhoneEntry.Text?.Trim() ?? "";
        SummaryEmail.Text = phone.Length > 0 ? $"{EmailEntry.Text?.Trim()} · {phone}" : EmailEntry.Text?.Trim();
    }

    private void OnEditServiceTapped(object? sender, TappedEventArgs e) => ShowStep(1);
    private void OnEditDetailsTapped(object? sender, TappedEventArgs e) => ShowStep(2);

    private void OnMessageChanged(object? sender, TextChangedEventArgs e)
    {
        CountLabel.Text = $"{e.NewTextValue?.Length ?? 0} / 4000";
        StepError.IsVisible = false;
    }

    // ===================== sending =====================

    private async Task SendAsync()
    {
        // Disable the buttons so the request can't be sent twice
        NextButton.IsEnabled = BackButton.IsEnabled = false;
        NextButton.Text = "Sending...";

        var result = await _api.SendQuoteAsync(new QuoteRequest
        {
            Name = $"{FirstNameEntry.Text?.Trim()} {LastNameEntry.Text?.Trim()}".Trim(),
            Email = EmailEntry.Text?.Trim() ?? "",
            Phone = PhoneEntry.Text?.Trim() ?? "",
            Service = _service,
            Message = MessageEditor.Text?.Trim() ?? ""
        });

        NextButton.IsEnabled = BackButton.IsEnabled = true;
        NextButton.Text = "Send request";

        if (!result.Success)
        {
            StepError.Text = result.Error;
            StepError.IsVisible = true;
            return;
        }

        Haptics.Success();
        ShowDone(result.Queued);
    }

    // The "Request sent" screen (instead of a pop-up)
    private void ShowDone(bool queued)
    {
        Step1.IsVisible = Step2.IsVisible = Step3.IsVisible = false;
        StepError.IsVisible = false;
        Header.IsVisible = false;
        ActionBar.IsVisible = false;
        DonePanel.IsVisible = true;

        if (queued)
        {
            DoneTitle.Text = "Saved on your phone";
            DoneText.Text = "There is no connection right now. Your request is saved and will be sent automatically when you are back online.";
        }
        else
        {
            DoneTitle.Text = "Request sent!";
            DoneText.Text = "Thank you. Our team will look at your request and get back to you within one business day.";
        }

        TrackButton.Text = ClientSession.IsLoggedIn ? "Track it in my account" : "Create an account to track it";
        ScrollToTop();
    }

    // Empties everything for a new request
    private void ResetForm()
    {
        _service = "";
        LastNameEntry.Text = PhoneEntry.Text = string.Empty;
        MessageEditor.Text = string.Empty;
        if (!ClientSession.IsLoggedIn)
            FirstNameEntry.Text = EmailEntry.Text = string.Empty;
        else
            PrefillFromAccount();
        ShowStep(1);
    }

    private async void OnTrackClicked(object? sender, EventArgs e)
    {
        ResetForm();
        await AppNavigation.GoToAccountAsync();
    }

    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        ResetForm();
        await AppNavigation.GoToTabAsync("Home");
    }

    private void OnNewRequestTapped(object? sender, TappedEventArgs e) => ResetForm();

    // Checks that an email looks like name@something.com
    private static bool IsValidEmail(string email)
    {
        try
        {
            return email.Length > 0 && new MailAddress(email).Address == email;
        }
        catch
        {
            return false;
        }
    }

    // "Prefer to talk?" buttons: open the phone's dialler, WhatsApp, mail app or maps
    private async void OnCallClicked(object? sender, EventArgs e) => await QuickContact.CallAsync();
    private async void OnWhatsAppClicked(object? sender, EventArgs e) => await QuickContact.WhatsAppAsync();
    private async void OnEmailClicked(object? sender, EventArgs e) => await QuickContact.EmailAsync();
    private async void OnAddressTapped(object? sender, TappedEventArgs e) => await QuickContact.DirectionsAsync();
}
