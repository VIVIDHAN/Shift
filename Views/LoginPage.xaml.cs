using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using DeliveryApp.Services;

namespace DeliveryApp.Views
{
    public partial class LoginPage : ContentPage
    {
        private bool isAdminMode = false;

        public LoginPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.Opacity = 0;
            this.TranslationY = 50;
            await Task.WhenAll(
                this.FadeTo(1, 800, Easing.CubicOut),
                this.TranslateTo(0, 0, 800, Easing.CubicOut)
            );
        }

        private void OnLanguageToggled(object sender, ToggledEventArgs e)
        {
            LocalizationManager.Instance.IsTamil = e.Value;
        }

        private void OnRoleDriverClicked(object sender, EventArgs e)
        {
            isAdminMode = false;
            DriverBtn.BackgroundColor = Color.FromArgb("#1E3A8A");
            DriverBtn.TextColor = Colors.White;
            AdminBtn.BackgroundColor = Colors.Transparent;
            AdminBtn.TextColor = Color.FromArgb("#6B7280");
            PhoneFrame.IsVisible = true;
            LoginSubtitleLabel.IsVisible = true;
            LoginTitleLabel.Text = LocalizationManager.Instance.IsTamil ? "ஓட்டுநர் உள்நுழைவு" : "Driver Login";
            SubmitLoginBtn.Text = LocalizationManager.Instance.IsTamil ? "உள்நுழைய" : "Start Shift";
        }

        private void OnRoleAdminClicked(object sender, EventArgs e)
        {
            isAdminMode = true;
            AdminBtn.BackgroundColor = Color.FromArgb("#1E3A8A");
            AdminBtn.TextColor = Colors.White;
            DriverBtn.BackgroundColor = Colors.Transparent;
            DriverBtn.TextColor = Color.FromArgb("#6B7280");
            PhoneFrame.IsVisible = false;
            LoginSubtitleLabel.IsVisible = false;
            LoginTitleLabel.Text = LocalizationManager.Instance.IsTamil ? "நிர்வாகி உள்நுழைவு" : "Admin Login";
            SubmitLoginBtn.Text = LocalizationManager.Instance.IsTamil ? "கட்டுப்பாட்டுப் பலகத்தைத் திற" : "Open ERP";
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (isAdminMode)
            {
                if (PasswordEntry.Text == "admin")
                {
                    await Navigation.PushAsync(new AdminPage());
                }
                else
                {
                    await DisplayAlert("Error", "Invalid admin password. Try 'admin'.", "OK");
                }
                return;
            }

            var phone = PhoneEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both phone and password", "OK");
                return;
            }

            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://10.0.2.2:5287");

                var loginData = new { PhoneNumber = phone, Password = password };
                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/driver/login", content);

                if (response.IsSuccessStatusCode)
                {
                    Preferences.Set("DriverPhone", phone);
                    Application.Current.MainPage = new NavigationPage(new DashboardPage(phone));
                }
                else
                {
                    await DisplayAlert("Login Failed", "Invalid credentials", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Network Error", "Cannot connect to server. Ensure API is running.", "OK");
            }
        }
    }
}
