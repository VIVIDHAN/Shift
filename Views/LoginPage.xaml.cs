namespace DeliveryApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        LanguageToggle.IsToggled = Services.LocalizationManager.Instance.IsTamil;
    }

    private void OnLanguageToggled(object sender, ToggledEventArgs e)
    {
        Services.LocalizationManager.Instance.IsTamil = e.Value;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Content.Opacity = 0;
        this.Content.TranslationY = 50;
        
        await Task.WhenAll(
            this.Content.FadeTo(1, 800, Easing.CubicOut),
            this.Content.TranslateTo(0, 0, 800, Easing.CubicOut)
        );
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
            navPage.BarBackgroundColor = Color.FromArgb("#1E3A8A");
            navPage.BarTextColor = Colors.White;
            Application.Current.MainPage = navPage;
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid phone number or password.", "OK");
        }
    }
}
