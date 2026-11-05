// Pages use it like this:
//   ErrorBox.Show("Could not reach the server");   -> red banner + Retry
//   ErrorBox.ShowInfo("You're offline ...");       -> soft blue note
//   ErrorBox.Hide();                                -> hide it
//   ErrorBox.Retry += ...                           -> what to do when Retry is tapped
// The colours are theme colours (see ThemeService.cs), so they follow dark / light mode.
namespace AndiswaConsultationApp.Controls;

public partial class ErrorBox : ContentView
{
    public event EventHandler? Retry;

    public ErrorBox()
    {
        InitializeComponent();
    }

    // A real problem (red banner with a "Retry" button)
    public void Show(string message)
    {
        Box.SetDynamicResource(VisualElement.BackgroundColorProperty, "ErrorBg");
        MessageLabel.SetDynamicResource(Label.TextColorProperty, "ErrorText");
        IconLabel.SetDynamicResource(Label.TextColorProperty, "ErrorText");
        IconLabel.Text = "!";
        MessageLabel.Text = message;
        RetryButton.IsVisible = true;
        IsVisible = true;
    }

    // Just a friendly note, for example "you are offline" (soft banner, no button)
    public void ShowInfo(string message)
    {
        Box.SetDynamicResource(VisualElement.BackgroundColorProperty, "PrimarySoft");
        MessageLabel.SetDynamicResource(Label.TextColorProperty, "Ink");
        IconLabel.SetDynamicResource(Label.TextColorProperty, "Primary");
        IconLabel.Text = "i";
        MessageLabel.Text = message;
        RetryButton.IsVisible = false;
        IsVisible = true;
    }

    public void Hide() => IsVisible = false;

    private void OnRetryClicked(object? sender, EventArgs e) => Retry?.Invoke(this, EventArgs.Empty);
}
