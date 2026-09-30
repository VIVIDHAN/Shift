using DeliveryApp.Models;
using DeliveryApp.Services;
using System.IO;

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
            byte[] photoData = null;
            if (_photoResult != null)
            {
                using var stream = await _photoResult.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                photoData = memoryStream.ToArray();
            }

            // 1. Update Database via our new Local API
            var apiService = new ApiService();
            string driverPhone = Preferences.Get("DriverPhone", "Unknown");
            bool dbSuccess = await apiService.CompleteDeliveryAsync(_currentOrder.OrderId, driverPhone, photoData);
            
            if (!dbSuccess)
            {
                await DisplayAlert("Error", "Failed to update database. Please try again.", "OK");
                return;
            }

            // 2. Fetch SMS Template
            var settings = await apiService.GetSettingsAsync();
            string finalMsg = "";
            if (settings != null && !string.IsNullOrEmpty(settings.SmsTemplate))
            {
                finalMsg = settings.SmsTemplate
                    .Replace("{OrderId}", _currentOrder.OrderId)
                    .Replace("{CustomerName}", _currentOrder.CustomerName)
                    .Replace("{Items}", _currentOrder.Items);
            }
            else
            {
                // Fallback template
                string englishMsg = $"Order {_currentOrder.OrderId} has been successfully delivered. Items: {_currentOrder.Items}.";
                string tamilMsg = $"ஆர்டர் {_currentOrder.OrderId} வெற்றிகரமாக வழங்கப்பட்டது. பொருட்கள்: {_currentOrder.Items}.";
                finalMsg = $"{englishMsg}\n{tamilMsg}";
            }

            var recipients = new List<string> { _currentOrder.CustomerPhone, _currentOrder.ShopOwnerPhone };

            bool shouldSend = await DisplayAlert("Notify Customer", "Do you want to send a delivery confirmation SMS to the customer and shop owner?", "Yes, Send", "No, Skip");

            if (shouldSend)
            {
#if ANDROID
                // Send SMS directly on Android
                try
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.Sms>();
                    }
                    
                    if (status == PermissionStatus.Granted)
                    {
                        var smsManager = Android.Telephony.SmsManager.Default;
                        foreach (var number in recipients)
                        {
                            if (!string.IsNullOrEmpty(number))
                            {
                                smsManager.SendTextMessage(number, null, finalMsg, null, null);
                            }
                        }
                        await DisplayAlert("Sent", "SMS sent successfully in the background.", "OK");
                    }
                    else
                    {
                        // Fallback if permission denied
                        await Sms.Default.ComposeAsync(new SmsMessage(finalMsg, recipients));
                    }
                }
                catch (Exception smsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Direct SMS Failed: {smsEx.Message}");
                    await Sms.Default.ComposeAsync(new SmsMessage(finalMsg, recipients));
                }
#else
                await Sms.Default.ComposeAsync(new SmsMessage(finalMsg, recipients));
#endif
            }

            await DisplayAlert("Success", "Delivery marked as complete!", "OK");
            
            // Navigate back to Dashboard
            await Navigation.PopAsync();
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("SMS Error", "SMS is not supported on this device (Emulator). Database was updated successfully.", "OK");
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
}
