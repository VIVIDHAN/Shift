namespace DeliveryApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PhoneEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please enter both phone number and password.", "OK");
            return;
        }

        var apiService = new Services.ApiService();
        bool success = await apiService.DriverLoginAsync(PhoneEntry.Text, PasswordEntry.Text);
        
        if (success)
        {
            // Save driver phone number for SMS sending later
            Preferences.Set("DriverPhone", PhoneEntry.Text);

            // Redirect to Dashboard
            var navPage = new NavigationPage(new DashboardPage());
            navPage.BarBackgroundColor = Color.FromArgb("#F3F4F6");
            navPage.BarTextColor = Color.FromArgb("#111827");
            Application.Current.MainPage = navPage;
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid phone number or password.", "OK");
        }
    }
}
