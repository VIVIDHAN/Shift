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
            
            // Auto focus phone number
            PhoneEntry.Focus();
        }

        private void OnLanguageToggledAlt(object sender, EventArgs e)
        {
            LocalizationManager.Instance.IsTamil = !LocalizationManager.Instance.IsTamil;
            bool isTamil = LocalizationManager.Instance.IsTamil;
            
            // Update toggle UI
            if (isTamil)
            {
                EnBorder.BackgroundColor = Colors.White;
                EnLabel.TextColor = Color.Parse("#4B5563");
                TaBorder.BackgroundColor = Color.Parse("#1E4ED8");
                TaLabel.TextColor = Colors.White;
            }
            else
            {
                EnBorder.BackgroundColor = Color.Parse("#1E4ED8");
                EnLabel.TextColor = Colors.White;
                TaBorder.BackgroundColor = Colors.White;
                TaLabel.TextColor = Color.Parse("#4B5563");
            }
            
            PhoneEntry.Placeholder = isTamil ? "தொலைபேசி எண்" : "Enter mobile number";
            LoginTitleLabel.Text = isTamil ? "ஓட்டுநர் உள்நுழைவு" : "Welcome back, driver!";
            LoginBtnLabel.Text = isTamil ? "உள்நுழைய" : "Login";
            LoginSubtitleLabel.Text = isTamil ? "உங்கள் விநியோகங்களை தொடரவும்" : "Login to continue your deliveries";
        }
        
        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            // Material icon: visibility (e8f4) and visibility_off (e8f5)
            TogglePasswordLabel.Text = PasswordEntry.IsPassword ? "\ue8f4" : "\ue8f5";
        }

        private async void OnAdminLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdminPage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var phone = PhoneEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both phone and password", "OK");
                return;
            }
            
            SubmitLoginBtn.IsEnabled = false;

            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://15.206.179.78");

                var loginData = new { driverPhone = phone, action = password };
                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/orders/driver/login", content);

                if (response.IsSuccessStatusCode)
                {
                    Preferences.Set("DriverPhone", phone);
                    
                    // Show success animation
                    SubmitLoginBtn.IsVisible = false;
                    SuccessAnimView.IsVisible = true;
                    
                    // Pop animation for the icon
                    await SuccessAnimView.ScaleTo(1.2, 150, Easing.CubicOut);
                    await SuccessAnimView.ScaleTo(1.0, 150, Easing.CubicIn);
                    
                    await Task.Delay(800);
                    
                    Application.Current.MainPage = new NavigationPage(new DashboardPage());
                }
                else
                {
                    SubmitLoginBtn.IsEnabled = true;
                    await DisplayAlert("Login Failed", "Invalid credentials", "OK");
                }
            }
            catch (Exception ex)
            {
                SubmitLoginBtn.IsEnabled = true;
                await DisplayAlert("Network Error", "Cannot connect to server. Ensure API is running.", "OK");
            }
        }
    }
}
