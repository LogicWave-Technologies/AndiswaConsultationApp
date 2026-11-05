// A custom bottom navigation bar for switching between the app's main pages.
using AndiswaConsultationApp.Services;
using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace AndiswaConsultationApp.Controls;

public class BottomNavBar : ContentView
{
    // One button of the bar: the tab route, the text under the icon, and the icon drawing
    private record NavItem(string Route, string Title, string IconData);

    private const string QuoteRoute = "Contact";

    private static readonly NavItem[] Items =
    {
        new("Home", "Home",
            "M3,9 L12,2 L21,9 V20 A2,2 0 0 1 19,22 H5 A2,2 0 0 1 3,20 Z M9,22 V12 H15 V22"),
        new("Services", "Services",
            "M12,3 L14.5,8.5 L20.5,9.2 L16,13.3 L17.2,19.2 L12,16.2 L6.8,19.2 L8,13.3 L3.5,9.2 L9.5,8.5 Z"),
        new(QuoteRoute, "Quote", "M12,5 V19 M5,12 H19"),
        new("Portfolio", "Work",
            "M5,3 H19 A2,2 0 0 1 21,5 V19 A2,2 0 0 1 19,21 H5 A2,2 0 0 1 3,19 V5 A2,2 0 0 1 5,3 Z M9,7 A2,2 0 1 1 9,11 A2,2 0 1 1 9,7 Z M21,15 L17.9,11.9 A2,2 0 0 0 15.1,11.9 L6,21"),
        new("More", "More",
            "M4,6 H20 M4,12 H20 M4,18 H14"),
    };

    // The parts of each normal button, so we can recolour them when the tab changes
    private readonly List<(NavItem Item, Border Pill, Path Icon, Label Text)> _buttons = new();

    public BottomNavBar()
    {
        var grid = new Grid { Padding = new Thickness(6, 8, 6, 10) };

        for (int i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));   // equal width for every button

            View button = item.Route == QuoteRoute ? BuildQuoteButton(item) : BuildTabButton(item);

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await OnItemTappedAsync(item);
            button.GestureRecognizers.Add(tap);
            SemanticProperties.SetDescription(button, item.Title);   // read out by screen readers

            grid.Add(button, i, 0);
        }

        // The bar itself: a Surface strip with a thin line on top
        Content = new VerticalStackLayout
        {
            Spacing = 0,
            Children =
            {
                new BoxView { HeightRequest = 1 }.Themed(BoxView.ColorProperty, "Line"),
                grid
            }
        }.Themed(VisualElement.BackgroundColorProperty, "Surface");

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // A normal tab: icon inside a "pill" + the name under it
    private View BuildTabButton(NavItem item)
    {
        var icon = MakeIcon(item.IconData);

        var pill = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            WidthRequest = 60,
            HeightRequest = 32,
            HorizontalOptions = LayoutOptions.Center,
            Content = icon
        };

        var text = new Label
        {
            FontSize = 11,
            Text = item.Title,
            HorizontalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.NoWrap
        };

        _buttons.Add((item, pill, icon, text));

        return new VerticalStackLayout
        {
            Spacing = 4,
            BackgroundColor = Colors.Transparent,   // so the whole area can be tapped, not only the icon
            VerticalOptions = LayoutOptions.Center,
            Children = { pill, text }
        };
    }

    // The coral "+" button in the middle
    private static View BuildQuoteButton(NavItem item)
    {
        var icon = MakeIcon(item.IconData);
        icon.Stroke = Colors.White;
        icon.StrokeThickness = 2.6;

        var circle = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            WidthRequest = 54,
            HeightRequest = 54,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = icon
        }.Themed(VisualElement.BackgroundColorProperty, "Accent");

        return new Grid
        {
            BackgroundColor = Colors.Transparent,
            Children = { circle }
        };
    }

    private static Path MakeIcon(string data) => new()
    {
        Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(data)!,
        StrokeThickness = 2,
        StrokeLineCap = PenLineCap.Round,
        StrokeLineJoin = PenLineJoin.Round,
        WidthRequest = 24,
        HeightRequest = 24,
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center
    };

    private void OnLoaded(object? sender, EventArgs e)
    {
        // Recolour when the user moves to another tab, or when dark mode is switched on / off
        if (Shell.Current is not null)
            Shell.Current.Navigated += OnShellNavigated;
        ThemeService.Changed += Refresh;
        Refresh();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.Navigated -= OnShellNavigated;
        ThemeService.Changed -= Refresh;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e) => Refresh();

    // Works out which tab is open, from the address, for example "//Portfolio" -> "Portfolio"
    private static string CurrentTab()
    {
        var location = Shell.Current?.CurrentState?.Location?.OriginalString ?? "";
        var first = location.TrimStart('/').Split('/').FirstOrDefault() ?? "";

        // The login, register and dashboard pages are opened from the More tab
        return first is AppNavigation.ClientLoginRoute or AppNavigation.ClientRegisterRoute
                     or AppNavigation.ClientDashboardRoute
            ? "More"
            : first;
    }

    // The open tab: blue icon on a soft blue pill. The others: grey, no pill.
    private void Refresh()
    {
        var current = CurrentTab();
        foreach (var (item, pill, icon, text) in _buttons)
        {
            var selected = item.Route == current;
            pill.Themed(VisualElement.BackgroundColorProperty, selected ? "PrimarySoft" : "Surface");
            icon.Themed(Shape.StrokeProperty, selected ? "Primary" : "Soft");
            text.Themed(Label.TextColorProperty, selected ? "Primary" : "Soft");
            text.SetDynamicResource(Label.FontFamilyProperty, selected ? "HeadingFont" : "AppFontFamily");
        }
    }

    private static async Task OnItemTappedAsync(NavItem item)
    {
        // Tapping the tab you are already on does nothing
        if (item.Route == CurrentTab())
            return;

        Haptics.Tap();
        await AppNavigation.GoToTabAsync(item.Route);
    }
}
