// A reusable themed message banner that displays errors, information, and retry actions.
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
