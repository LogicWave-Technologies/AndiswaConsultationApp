// ============================================================================
// ProjectDetailPage.xaml.cs  -  the CODE behind the project detail page
// ----------------------------------------------------------------------------
// [QueryProperty(...)] means: "when someone opens this page with GoToAsync and passes a value
// called Project, put it in my Project property". AppNavigation.OpenProjectAsync does exactly that.
// ============================================================================
using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;

namespace AndiswaConsultationApp.Pages;

[QueryProperty(nameof(Project), "Project")]
public partial class ProjectDetailPage : ContentPage
{
    private ProjectItem? _project;

    // The project to show. When it arrives, show it on screen.
    public ProjectItem? Project
    {
        get => _project;
        set
        {
            _project = value;
            BindingContext = value;                // {Binding Title} etc. now read from this project
            if (value is not null)
                Title = value.Title;
        }
    }

    public ProjectDetailPage()
    {
        InitializeComponent();
    }

    // "Request a similar design": go to the Quote screen with this project's service already chosen
    private async void OnRequestClicked(object? sender, EventArgs e)
    {
        Haptics.Tap();
        await AppNavigation.RequestServiceAsync(_project?.ContactService ?? "Graphic Design");
    }

    // Opens the phone's own share menu (WhatsApp, SMS, email, ...). A website can't do this as nicely.
    private async Task ShareAsync()
    {
        if (_project is null) return;
        Haptics.Tap();

        var phone = AppState.Studio?.Phone is { Length: > 0 } p ? p : SiteContent.Phone;
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Share this design",
                Subject = $"{_project.Title} by Andiswa Business Consultation",
                Text = $"Look at this {_project.Category.ToLower()} design: \"{_project.Title}\" by Andiswa Business Consultation. " +
                       $"Want one too? Call or WhatsApp {phone}."
            });
        }
        catch
        {
            // Sharing not available on this device: nothing to do
        }
    }

    private async void OnShareClicked(object? sender, EventArgs e) => await ShareAsync();
    private async void OnShareTapped(object? sender, TappedEventArgs e) => await ShareAsync();

    // The round back arrow on the picture
    private async void OnBackTapped(object? sender, TappedEventArgs e) =>
        await AppNavigation.BackAsync();
}
