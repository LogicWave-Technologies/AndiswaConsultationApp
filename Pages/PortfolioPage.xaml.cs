using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AndiswaConsultationApp.Pages;

public partial class PortfolioPage : ContentPage
{
    // The filter chips: the first part is the "Filter" value of a project, the second is the text on the chip
    private static readonly (string Key, string Text)[] Filters =
    {
        ("all", "All"),
        ("branding", "Branding"),
        ("flyers", "Flyers"),
        ("posters", "Posters"),
        ("social", "Social"),
    };

    // Tile heights used in turn, so the two columns look like a "masonry" wall
    private static readonly double[] TileHeights = { 230, 180, 200, 250 };

    private readonly Dictionary<string, (Border Chip, Label Text)> _chips = new();
    private string _filter = "all";

    public PortfolioPage()
    {
        InitializeComponent();
        BuildFilters();
        ApplyFilter();
    }

    // While this page is on screen, re-colour the chips if dark mode is switched
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ThemeService.Changed += UpdateChipColours;
        UpdateChipColours();      // also catches a change made while the page was away
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ThemeService.Changed -= UpdateChipColours;
    }

    private void BuildFilters()
    {
        foreach (var (key, text) in Filters)
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

            var chosen = key;     // remember which filter THIS chip belongs to
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                Haptics.Tap();
                _filter = chosen;
                UpdateChipColours();
                ApplyFilter();
            };
            chip.GestureRecognizers.Add(tap);

            _chips[key] = (chip, label);
            FilterLayout.Children.Add(chip);
        }

        UpdateChipColours();
    }

    // Selected chip = blue with white text, the others = card colour with grey text
    private void UpdateChipColours()
    {
        foreach (var (key, (chip, label)) in _chips)
        {
            var selected = key == _filter;
            chip.BackgroundColor = ThemeService.Get(selected ? "Primary" : "Surface");
            chip.Stroke = ThemeService.Get(selected ? "Primary" : "Line");
            label.TextColor = ThemeService.Get(selected ? "OnPrimary" : "Muted");
        }
    }

    // Works out which projects match the filter AND the search text, then shows them
    private void ApplyFilter()
    {
        var search = SearchEntry.Text?.Trim() ?? "";

        var matches = SiteContent.Projects
            .Where(p => _filter == "all" || p.Filter == _filter)
            .Where(p => search.Length == 0
                        || p.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || p.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || p.Blurb.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        CountLabel.Text = matches.Count == 1 ? "1 design" : $"{matches.Count} designs";
        EmptyPanel.IsVisible = matches.Count == 0;
        BuildGallery(matches);
    }

    // Puts the tiles in two columns: 1st left, 2nd right, 3rd left ...
    private void BuildGallery(List<ProjectItem> projects)
    {
        LeftColumn.Children.Clear();
        RightColumn.Children.Clear();

        for (var i = 0; i < projects.Count; i++)
        {
            var tile = BuildTile(projects[i], TileHeights[i % TileHeights.Length]);
            (i % 2 == 0 ? LeftColumn : RightColumn).Children.Add(tile);
        }
    }

    // One picture tile. The picture fills the tile (the detail page shows the whole design).
    private static Border BuildTile(ProjectItem project, double height)
    {
        var fade = new BoxView
        {
            VerticalOptions = LayoutOptions.End,
            HeightRequest = 90,
            Background = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop(Color.FromArgb("#00000000"), 0),
                new GradientStop(Color.FromArgb("#CC0B111C"), 1)
            }, new Point(0, 0), new Point(0, 1))
        };

        var texts = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.End,
            Padding = new Thickness(12),
            Spacing = 0,
            Children =
            {
                new Label { Text = project.Category, FontSize = 11, TextColor = Color.FromArgb("#FFC9B3") },
                new Label
                {
                    Text = project.Title,
                    FontSize = 14,
                    TextColor = Colors.White,
                    MaxLines = 2,
                    LineBreakMode = LineBreakMode.TailTruncation
                }.Themed(Label.FontFamilyProperty, "HeadingFont")
            }
        };

        var tile = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            HeightRequest = height,
            Content = new Grid
            {
                Children =
                {
                    new Image { Source = ImageSource.FromFile(project.Image), Aspect = Aspect.AspectFill },
                    fade,
                    texts
                }
            }
        }.Themed(VisualElement.BackgroundColorProperty, "SurfaceAlt");

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await AppNavigation.OpenProjectAsync(project);
        tile.GestureRecognizers.Add(tap);
        SemanticProperties.SetDescription(tile, project.Title);

        return tile;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();
}
