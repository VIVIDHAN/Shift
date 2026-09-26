namespace DeliveryApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a valid phone number.", "OK");
            return;
        }

        // Save driver phone number for SMS sending later
        Preferences.Set("DriverPhone", PhoneEntry.Text);

        // Redirect to Dashboard (no OTP as per requirement)
        Application.Current.MainPage = new NavigationPage(new DashboardPage());
    }
}
