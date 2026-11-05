// ============================================================================
// TeamPage.xaml.cs  -  the CODE behind the Team page
// ----------------------------------------------------------------------------
// Only one thing to do: "Send an email" opens the phone's mail app.
// ============================================================================
using AndiswaConsultationApp.Models;
using AndiswaConsultationApp.Services;

namespace AndiswaConsultationApp.Pages;

public partial class TeamPage : ContentPage
{
    public TeamPage()
    {
        InitializeComponent();
    }

    // "sender" is the button; its BindingContext is the TeamMember that card shows
    private async void OnEmailClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject button && button.BindingContext is TeamMember member)
        {
            Haptics.Tap();
            try { await Launcher.Default.OpenAsync($"mailto:{member.Email}"); } catch { }
        }
    }
}
