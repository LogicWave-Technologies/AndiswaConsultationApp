// ============================================================================
// Haptics.cs  -  a tiny vibration when an important button is tapped
// ============================================================================
namespace AndiswaConsultationApp.Services;

public static class Haptics
{
    // A light tap (buttons, tiles)
    public static void Tap()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    // A longer buzz (something important happened, e.g. the quote was sent)
    public static void Success()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
    }
}
