using DeliveryApp.Models;
using DeliveryApp.Services;

namespace DeliveryApp.Views;

public partial class DeliveryPage : ContentPage
{
    private Order _currentOrder;
    private FileResult _photoResult;

    public DeliveryPage(Order order)
    {
        InitializeComponent();
        _currentOrder = order;

        OrderIdLabel.Text = $"Order ID: {order.OrderId}";
        ItemsLabel.Text = $"Items: {order.Items}";
        CustomerLabel.Text = $"Customer: {order.CustomerName}";
        LocationLabel.Text = $"Location: {order.Location}";
    }

    private async void OnClickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                _photoResult = await MediaPicker.Default.CapturePhotoAsync();

                if (_photoResult != null)
                {
                    var stream = await _photoResult.OpenReadAsync();
                    CapturedImage.Source = ImageSource.FromStream(() => stream);
                    CapturedImage.IsVisible = true;
                }
            }
            else
            {
                await DisplayAlert("Error", "Camera is not supported on this device.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_photoResult == null)
        {
            await DisplayAlert("Photo Required", "Please click a photo of the delivered item before submitting.", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            // 1. Request SMS Permission and Trigger SMS in background
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
            }

            if (status == PermissionStatus.Granted)
            {
                var smsService = DependencyService.Get<ISmsService>();
                if (smsService != null)
                {
                    string englishMsg = $"Order {_currentOrder.OrderId} has been successfully delivered. Items: {_currentOrder.Items}.";
                    string tamilMsg = $"ஆர்டர் {_currentOrder.OrderId} வெற்றிகரமாக வழங்கப்பட்டது. பொருட்கள்: {_currentOrder.Items}.";
                    string finalMsg = $"{englishMsg}\n{tamilMsg}";

                    smsService.SendSmsInBackground(_currentOrder.CustomerPhone, finalMsg);
                    smsService.SendSmsInBackground(_currentOrder.ShopOwnerPhone, finalMsg);
                }
            }
            else
            {
                await DisplayAlert("Permission Denied", "SMS permission is required to send delivery confirmation.", "OK");
                return;
            }

            // 2. Mock Database Update via AWS Lambda API
            await UpdateDatabaseMockAsync();

            await DisplayAlert("Success", "Delivery submitted and SMS sent successfully!", "OK");
            
            // Navigate back to Dashboard
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Submission failed: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async Task UpdateDatabaseMockAsync()
    {
        try
        {
            var payload = new 
            {
                orderId = _currentOrder.OrderId,
                status = "Delivered",
                driverPhone = Preferences.Get("DriverPhone", "Unknown")
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            
            HttpClient client = new HttpClient();
            var response = await client.PostAsync("https://shvfrpgoe7hvwgcv57ua4edeku0kymaj.lambda-url.ap-south-1.on.aws/", content);
            
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("Successfully updated backend database.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update backend: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Backend error: {ex.Message}");
        }
    }
}
