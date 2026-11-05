// ============================================================================
// Haptics.cs  -  a tiny vibration when an important button is tapped
// ----------------------------------------------------------------------------
// This is something a website cannot do: the phone gives a small "click"
// you can feel. Used on the Quote button, the service tiles, sending a form ...
// If the phone (or Windows) has no vibration, nothing happens.
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
