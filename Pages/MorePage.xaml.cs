// ============================================================================
// MorePage.xaml.cs  -  builds the grouped rows of the More tab
// ----------------------------------------------------------------------------
//   AddRow(...)  = one row: icon, title, small text, ">" arrow
//   AddFaq(...)  = one question; tap it and the answer opens under it
// ============================================================================
using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AndiswaConsultationApp.Pages;

public partial class MorePage : ContentPage
{
    public MorePage()
    {
        InitializeComponent();

        // THE STUDIO
        AddRow(StudioGroup, "🏢", "About us", "Our story, mission and vision", () => AppNavigation.OpenPageAsync("about"));
        AddRow(StudioGroup, "👥", "Team", "The people behind every brand", () => AppNavigation.OpenPageAsync("team"));
        AddRow(StudioGroup, "⭐", "Reviews", "What our clients say", () => AppNavigation.OpenPageAsync("testimonials"), last: true);

        // HELP: the quick answers (from the website's chat assistant) open right here
        foreach (var item in SiteContent.Faq)
            AddFaq(HelpGroup, item);
        AddRow(HelpGroup, "💬", "Chat on WhatsApp", "Usually replies within the day", () => QuickContact.WhatsAppAsync());
        AddRow(HelpGroup, "📞", "Call the studio", SiteContent.Hours, () => QuickContact.CallAsync(), last: true);

        VersionLabel.Text = $"Andiswa Consultation · version {AppInfo.Current.VersionString}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        DarkSwitch.IsToggled = ThemeService.IsDark;
        FontNameLabel.Text = ThemeService.Fonts.FirstOrDefault(f => f.Family == ThemeService.FontFamily).Name ?? "Poppins";

        // The account card changes when you are logged in
        await ClientSession.LoadAsync();
        if (ClientSession.IsLoggedIn)
        {
            AccountName.Text = ClientSession.Name.Length > 0 ? ClientSession.Name : "My account";
            AccountSub.Text = "My quotes and projects";
            AccountInitials.Text = string.Concat(ClientSession.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                                  .Take(2).Select(p => char.ToUpper(p[0])));
            if (AccountInitials.Text.Length == 0) AccountInitials.Text = "★";
        }
        else
        {
            AccountName.Text = "Sign in or create an account";
            AccountSub.Text = "Track your quotes and projects";
            AccountInitials.Text = "?";
        }
    }

    // ---- one tappable row ----
    private static void AddRow(VerticalStackLayout group, string icon, string title, string subtitle,
                               Func<Task> onTap, bool last = false)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(40),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 14,
            Padding = new Thickness(16, 12),
            BackgroundColor = Colors.Transparent
        };

        row.Add(IconBox(new Label
        {
            Text = icon,
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        }), 0, 0);

        row.Add(new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = title, FontSize = 14 }
                    .Themed(Label.TextColorProperty, "Ink").Themed(Label.FontFamilyProperty, "HeadingFont"),
                new Label { Text = subtitle, FontSize = 12 }.Themed(Label.TextColorProperty, "Muted")
            }
        }, 1, 0);

        row.Add(Chevron(), 2, 0);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            Haptics.Tap();
            await onTap();
        };
        row.GestureRecognizers.Add(tap);
        SemanticProperties.SetDescription(row, title);

        group.Children.Add(row);
        if (!last)
            group.Children.Add(Divider());
    }

    // ---- one question that opens / closes ----
    private static void AddFaq(VerticalStackLayout group, FaqItem item)
    {
        var sign = new Label { Text = "+", FontSize = 20, VerticalOptions = LayoutOptions.Center }
            .Themed(Label.TextColorProperty, "Primary").Themed(Label.FontFamilyProperty, "HeadingFont");

        var answer = new Label
        {
            Text = item.Answer,
            FontSize = 13,
            LineHeight = 1.4,
            IsVisible = false,
            Margin = new Thickness(54, 0, 16, 14)
        }.Themed(Label.TextColorProperty, "Muted");

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(40),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 14,
            Padding = new Thickness(16, 12),
            BackgroundColor = Colors.Transparent
        };
        row.Add(IconBox(new Label
        {
            Text = "?",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        }.Themed(Label.TextColorProperty, "Primary").Themed(Label.FontFamilyProperty, "HeadingFont")), 0, 0);
        row.Add(new Label { Text = item.Question, FontSize = 14, VerticalOptions = LayoutOptions.Center }
            .Themed(Label.TextColorProperty, "Ink"), 1, 0);
        row.Add(sign, 2, 0);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            Haptics.Tap();
            answer.IsVisible = !answer.IsVisible;
            sign.Text = answer.IsVisible ? "–" : "+";
        };
        row.GestureRecognizers.Add(tap);

        group.Children.Add(new VerticalStackLayout { Spacing = 0, Children = { row, answer } });
        group.Children.Add(Divider());
    }

    private static Border IconBox(View content) => new Border
    {
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = 12 },
        WidthRequest = 40,
        HeightRequest = 40,
        Content = content
    }.Themed(VisualElement.BackgroundColorProperty, "PrimarySoft");

    private static BoxView Divider() =>
        new BoxView { HeightRequest = 1, Margin = new Thickness(70, 0, 0, 0) }.Themed(BoxView.ColorProperty, "Line");

    private static Microsoft.Maui.Controls.Shapes.Path Chevron() => new Microsoft.Maui.Controls.Shapes.Path
    {
        Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString("M9,18 L15,12 L9,6")!,
        StrokeThickness = 2,
        StrokeLineCap = PenLineCap.Round,
        StrokeLineJoin = PenLineJoin.Round,
        WidthRequest = 24,
        HeightRequest = 24,
        VerticalOptions = LayoutOptions.Center
    }.Themed(Shape.StrokeProperty, "Soft");

    // ---- taps ----
    private async void OnAccountTapped(object? sender, TappedEventArgs e)
    {
        Haptics.Tap();
        await AppNavigation.GoToAccountAsync();
    }

    private async void OnSettingsTapped(object? sender, TappedEventArgs e) =>
        await AppNavigation.OpenPageAsync("settings");

    private void OnDarkToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value != ThemeService.IsDark)
            ThemeService.SetDark(e.Value);
    }
}
