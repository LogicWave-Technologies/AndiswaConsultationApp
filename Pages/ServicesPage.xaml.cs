// ============================================================================
// ServicesPage.xaml.cs  -  the CODE behind the Services tab
// ----------------------------------------------------------------------------
//   - builds the filter chips and shows only the services of the chosen chip
//   - tapping a service opens the Quote screen with that service already chosen
// ============================================================================
using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AndiswaConsultationApp.Pages;

public partial class ServicesPage : ContentPage
{
    // Chip text + which Contact-form services it shows (empty = all)
    private static readonly (string Text, string[] ContactServices)[] Chips =
    {
        ("All", Array.Empty<string>()),
        ("Branding", new[] { "Branding & Identity" }),
        ("Graphic", new[] { "Graphic Design" }),
        ("Digital", new[] { "Social Media Design", "Web Design" }),
        ("Business", new[] { "Business Consultation" }),
    };

    private readonly Dictionary<string, (Border Chip, Label Text)> _chips = new();
    private string _chosen = "All";

    public ServicesPage()
    {
        InitializeComponent();
        BuildChips();
        ApplyFilter();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ThemeService.Changed += RefreshChips;
        RefreshChips();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ThemeService.Changed -= RefreshChips;
    }

    private void BuildChips()
    {
        foreach (var (text, _) in Chips)
        {
            var label = new Label { Text = text, FontSize = 13, VerticalOptions = LayoutOptions.Center }
                .Themed(Label.FontFamilyProperty, "HeadingFont");

            var chip = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                StrokeThickness = 1,
                Padding = new Thickness(16, 8),
                Content = label
            };

            var name = text;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                Haptics.Tap();
                _chosen = name;
                RefreshChips();
                ApplyFilter();
            };
            chip.GestureRecognizers.Add(tap);

            _chips[text] = (chip, label);
            ChipLayout.Children.Add(chip);
        }
        RefreshChips();
    }

    // Selected chip = blue with white text, the others = card colour with grey text
    private void RefreshChips()
    {
        foreach (var (text, (chip, label)) in _chips)
        {
            var selected = text == _chosen;
            chip.BackgroundColor = ThemeService.Get(selected ? "Primary" : "Surface");
            chip.Stroke = ThemeService.Get(selected ? "Primary" : "Line");
            label.TextColor = ThemeService.Get(selected ? "OnPrimary" : "Muted");
        }
    }

    private void ApplyFilter()
    {
        var wanted = Chips.First(c => c.Text == _chosen).ContactServices;
        var list = SiteContent.Services
            .Where(s => wanted.Length == 0 || wanted.Contains(s.ContactService))
            .ToList();
        BindableLayout.SetItemsSource(ServiceList, list);
    }

    // "sender" is the card; its BindingContext is the ServiceItem that card shows.
    private async void OnServiceTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject card && card.BindingContext is ServiceItem service)
        {
            Haptics.Tap();
            await AppNavigation.RequestServiceAsync(service.ContactService);
        }
    }

    private async void OnChatClicked(object? sender, EventArgs e) =>
        await QuickContact.WhatsAppAsync("Hi Andiswa, I'm not sure which service I need. Can you help me?");
}
