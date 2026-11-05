using AndiswaConsultationApp.Models;

namespace AndiswaConsultationApp.Pages;

// The reviews are shown by the XAML straight from SiteContent.Testimonials.
// Here we only write how many reviews there are under the score.
public partial class TestimonialsPage : ContentPage
{
    public TestimonialsPage()
    {
        InitializeComponent();
        CountLabel.Text = $"From {SiteContent.Testimonials.Count} happy clients";
    }
}
