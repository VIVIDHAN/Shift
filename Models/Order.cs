namespace DeliveryApp.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string ShopOwnerPhone { get; set; }
        public string Items { get; set; }
        public string Location { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // "Assigned", "Delivered"
    }

    public class ShopSettings
    {
        public string ShopOwnerPhone { get; set; }
        public string SmsTemplate { get; set; }
    }
}
