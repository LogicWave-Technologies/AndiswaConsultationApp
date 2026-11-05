// A reusable Google sign-in button for the login and registration pages.
using AndiswaConsultationApp.Services;
using Microsoft.Extensions.DependencyInjection;   // GetService<T>()

namespace AndiswaConsultationApp.Controls;

public class GoogleSignInButton : ContentView
{
    private readonly Button _button;
    private readonly VerticalStackLayout _waitingPanel;
    private readonly Label _errorLabel;
    private CancellationTokenSource? _cancel;   // lets the Cancel button stop the waiting

    public GoogleSignInButton()
    {
        _button = new Button
        {
            Text = "Continue with Google",
            Style = (Style)Application.Current!.Resources["OutlineButton"]
        };
        _button.Clicked += OnGoogleClicked;

        // Shown while we wait for the browser
        var cancelButton = new Button
        {
            Text = "Cancel",
            Style = (Style)Application.Current!.Resources["OutlineButton"],
            HeightRequest = 44
        };
        cancelButton.Clicked += (_, _) => _cancel?.Cancel();

        _waitingPanel = new VerticalStackLayout
        {
            Spacing = 12,
            IsVisible = false,
            Children =
            {
                new ActivityIndicator { IsRunning = true }.Themed(ActivityIndicator.ColorProperty, "Navy"),
                new Label
                {
                    Text = ApiConfig.TestMode
                        ? "Test mode: signing in a pretend Google user..."
                        : "Finish signing in with Google in your browser, then come back to the app.",
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center
                }.Themed(Label.TextColorProperty, "Muted"),
                cancelButton
            }
        };

        _errorLabel = new Label { FontSize = 14, IsVisible = false }
            .Themed(Label.TextColorProperty, "ErrorText");

        Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children = { _button, _waitingPanel, _errorLabel }
        };
    }

    private async void OnGoogleClicked(object? sender, EventArgs e)
    {
        // Get the shared ApiService (registered in MauiProgram.cs)
        var api = IPlatformApplication.Current?.Services.GetService<ApiService>();
        if (api is null)
            return;

        _errorLabel.IsVisible = false;
        SetWaiting(true);
        _cancel = new CancellationTokenSource();

        try
        {
            var (result, error) = await api.GoogleSignInAsync(_cancel.Token);

            if (result is null)
            {
                if (error.Length > 0)          // empty = the user pressed Cancel
                {
                    _errorLabel.Text = error;
                    _errorLabel.IsVisible = true;
                }
                return;
            }

            // Signed in: remember it and open the dashboard
            await ClientSession.SaveAsync(result.Token, result.Name, result.Email);
            await AppNavigation.GoToHomeAsync();
        }
        finally
        {
            SetWaiting(false);
            _cancel.Dispose();
            _cancel = null;
        }
    }

    private void SetWaiting(bool waiting)
    {
        _button.IsVisible = !waiting;
        _waitingPanel.IsVisible = waiting;
    }
}
