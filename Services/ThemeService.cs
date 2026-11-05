// ============================================================================
// ThemeService.cs  -  the APP's own look (colours + font), light and dark

// ============================================================================
namespace AndiswaConsultationApp.Services;

public static class ThemeService
{
    private const string DarkKey = "dark_mode";
    private const string FontKey = "font_family";

    // The app's own font (registered in MauiProgram.cs)
    public const string DefaultFont = "Poppins";

    /// <summary>Raised after a theme or font change.</summary>
    public static event Action? Changed;

    // Name shown on the Settings page, and the alias registered in MauiProgram.cs. "" = phone's normal font.
    public static readonly (string Name, string Family)[] Fonts =
    {
        ("Poppins (app default)", DefaultFont),
        ("System default", ""),
        ("Open Sans", "OpenSans"),
        ("Lora (serif)", "Lora"),
        ("Roboto Mono", "RobotoMono"),
    };

    private static readonly Dictionary<string, string> Light = new()
    {
        // ---- the app's own colours ----
        ["Primary"] = "#2D6CDF",       // sky blue: buttons, selected tab, links
        ["PrimaryDark"] = "#1C4FB0",
        ["PrimarySoft"] = "#E7EFFD",   // light blue behind icons and chips
        ["OnPrimary"] = "#FFFFFF",     // text on a Primary button
        ["Accent"] = "#FF7A45",        // warm coral: the floating Quote button, highlights
        ["AccentSoft"] = "#FFEDE4",
        ["PageBg"] = "#F3F6FB",
        ["Surface"] = "#FFFFFF",       // cards
        ["SurfaceAlt"] = "#EDF2F9",    // text boxes, tonal buttons
        ["Ink"] = "#101828",           // main text
        ["Muted"] = "#5D6B82",         // second text
        ["Soft"] = "#98A2B3",          // hints, inactive tabs
        ["Line"] = "#E4E9F2",
        ["Success"] = "#12A26B",
        ["SuccessSoft"] = "#E2F6EC",
        ["Warning"] = "#B97800",
        ["WarningSoft"] = "#FFF3D6",
        ["ErrorBg"] = "#FFF1F0",
        ["ErrorLine"] = "#F3C2BE",
        ["ErrorText"] = "#B42318",

        // ---- old names, kept so older code (e.g. GoogleSignInButton) still finds a colour ----
        ["Navy"] = "#2D6CDF",
        ["NavyDeep"] = "#1C4FB0",
        ["NavySoft"] = "#1C4FB0",
        ["Steel"] = "#2D6CDF",
        ["SteelLight"] = "#5B8DEF",
        ["Cream"] = "#FFFFFF",
        ["Mist"] = "#EDF2F9",
        ["OnDarkMuted"] = "#DCE6FA",
    };

    private static readonly Dictionary<string, string> Dark = new()
    {
        ["Primary"] = "#7FA8FF",
        ["PrimaryDark"] = "#5B8BF0",
        ["PrimarySoft"] = "#1B2A4A",
        ["OnPrimary"] = "#0B111C",
        ["Accent"] = "#FF8D5C",
        ["AccentSoft"] = "#3A2418",
        ["PageBg"] = "#0B111C",
        ["Surface"] = "#151D2C",
        ["SurfaceAlt"] = "#1D2739",
        ["Ink"] = "#EEF2F8",
        ["Muted"] = "#A3AEC2",
        ["Soft"] = "#6F7B90",
        ["Line"] = "#263247",
        ["Success"] = "#3DD69B",
        ["SuccessSoft"] = "#12352A",
        ["Warning"] = "#F2B542",
        ["WarningSoft"] = "#3A2E12",
        ["ErrorBg"] = "#3A1F24",
        ["ErrorLine"] = "#7A3B43",
        ["ErrorText"] = "#F2B8B8",

        ["Navy"] = "#7FA8FF",
        ["NavyDeep"] = "#0B111C",
        ["NavySoft"] = "#1D2739",
        ["Steel"] = "#7FA8FF",
        ["SteelLight"] = "#9CBCFF",
        ["Cream"] = "#F7F8FA",
        ["Mist"] = "#1D2739",
        ["OnDarkMuted"] = "#E3EAF3",
    };

    public static bool IsDark => Preferences.Default.Get(DarkKey, false);
    public static string FontFamily => Preferences.Default.Get(FontKey, DefaultFont);

    /// <summary>Call once when the app starts (App.xaml.cs).</summary>
    public static void Initialize() => Apply();

    public static void SetDark(bool dark)
    {
        Preferences.Default.Set(DarkKey, dark);
        Apply();
    }

    public static void SetFont(string family)
    {
        Preferences.Default.Set(FontKey, family);
        Apply();
    }

    private static void Apply()
    {
        var app = Application.Current;
        if (app is null) return;

        var resources = app.Resources;
        foreach (var (key, hex) in IsDark ? Dark : Light)
            resources[key] = Color.FromArgb(hex);

        // Body text uses the chosen font. Titles use the SemiBold version of Poppins
        // when Poppins is chosen, otherwise the same font as the body.
        resources["AppFontFamily"] = FontFamily;
        resources["HeadingFont"] = FontFamily == DefaultFont ? "PoppinsSemiBold" : FontFamily;

        app.UserAppTheme = IsDark ? AppTheme.Dark : AppTheme.Light;   // also darkens system pop-ups

        Changed?.Invoke();
    }

    /// <summary>Reads the CURRENT value of a theme colour.</summary>
    public static Color Get(string key) =>
        Application.Current!.Resources.TryGetValue(key, out var value) && value is Color c ? c : Colors.Magenta;

    /// <summary>
    /// For views built in C#: ties a property to a theme colour so it follows dark / light mode.
    /// Example: new Label { Text = "Hi" }.Themed(Label.TextColorProperty, "Primary")
    /// </summary>
    public static T Themed<T>(this T view, BindableProperty property, string resourceKey) where T : Element
    {
        view.SetDynamicResource(property, resourceKey);
        return view;
    }
}
