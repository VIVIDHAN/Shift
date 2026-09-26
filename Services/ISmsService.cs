namespace DeliveryApp.Services
{
    public interface ISmsService
    {
        void SendSmsInBackground(string phoneNumber, string message);
    }
}
