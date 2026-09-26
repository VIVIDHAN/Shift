#if ANDROID
using Android.Telephony;
using DeliveryApp.Services;
using Microsoft.Maui.Controls;

[assembly: Dependency(typeof(DeliveryApp.Platforms.Android.AndroidSmsService))]
namespace DeliveryApp.Platforms.Android
{
    public class AndroidSmsService : ISmsService
    {
        public void SendSmsInBackground(string phoneNumber, string message)
        {
            try
            {
                SmsManager smsManager = SmsManager.Default;
                smsManager.SendTextMessage(phoneNumber, null, message, null, null);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send SMS: {ex.Message}");
            }
        }
    }
}
#endif
